using System.Collections.Concurrent;

namespace BTPSecure.Server.Services;

// Évite une lecture en base à chaque requête authentifiée : le compte est-il actif,
// et quelle est sa session en cours ?
// Blocage et éviction restent immédiats : toute modification de EstActif ou de SessionId
// doit invalider l'entrée correspondante (sinon l'effet attendrait l'expiration).
// La durée de vie n'est qu'un filet de sécurité (invalidation ratée, plusieurs instances).
public class S_CacheComptes
{
    private static readonly TimeSpan _dureeDeVie = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<int, Entree> _entrees = new();

    public bool TryObtenir(int p_utilisateurId, out bool p_estActif, out string p_sessionId)
    {
        p_estActif = false;
        p_sessionId = string.Empty;

        Entree _entree;
        if (!_entrees.TryGetValue(p_utilisateurId, out _entree))
            return false;

        if (DateTime.UtcNow >= _entree.Expiration)
        {
            _entrees.TryRemove(p_utilisateurId, out _);
            return false;
        }

        p_estActif = _entree.EstActif;
        p_sessionId = _entree.SessionId;
        return true;
    }

    public void Definir(int p_utilisateurId, bool p_estActif, string p_sessionId)
    {
        var _entree = new Entree();
        _entree.EstActif = p_estActif;
        _entree.SessionId = p_sessionId;
        _entree.Expiration = DateTime.UtcNow.Add(_dureeDeVie);
        _entrees[p_utilisateurId] = _entree;
    }

    // À appeler dès qu'un compte est bloqué / débloqué OU qu'il ouvre une nouvelle
    // session : la prochaine requête relit la base
    public void Invalider(int p_utilisateurId)
    {
        _entrees.TryRemove(p_utilisateurId, out _);
    }

    private class Entree
    {
        public bool EstActif { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}
