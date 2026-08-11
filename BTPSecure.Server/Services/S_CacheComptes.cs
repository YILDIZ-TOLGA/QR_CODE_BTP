using System.Collections.Concurrent;

namespace BTPSecure.Server.Services;

// Évite une lecture en base à chaque requête authentifiée pour savoir si le compte est actif.
// Le blocage reste immédiat : toute modification de EstActif invalide l'entrée correspondante.
// La durée de vie n'est qu'un filet de sécurité (invalidation ratée, plusieurs instances).
public class S_CacheComptes
{
    private static readonly TimeSpan _dureeDeVie = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<int, Entree> _entrees = new();

    public bool TryObtenir(int p_utilisateurId, out bool p_estActif)
    {
        p_estActif = false;

        Entree _entree;
        if (!_entrees.TryGetValue(p_utilisateurId, out _entree))
            return false;

        if (DateTime.UtcNow >= _entree.Expiration)
        {
            _entrees.TryRemove(p_utilisateurId, out _);
            return false;
        }

        p_estActif = _entree.EstActif;
        return true;
    }

    public void Definir(int p_utilisateurId, bool p_estActif)
    {
        var _entree = new Entree();
        _entree.EstActif = p_estActif;
        _entree.Expiration = DateTime.UtcNow.Add(_dureeDeVie);
        _entrees[p_utilisateurId] = _entree;
    }

    // À appeler dès qu'un compte est bloqué / débloqué : la prochaine requête relit la base
    public void Invalider(int p_utilisateurId)
    {
        _entrees.TryRemove(p_utilisateurId, out _);
    }

    private class Entree
    {
        public bool EstActif { get; set; }
        public DateTime Expiration { get; set; }
    }
}
