using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

public class DAO_Parrainage
{
    private readonly AppDbContext _context;

    public DAO_Parrainage(AppDbContext p_context)
    {
        _context = p_context;
    }

    public async Task Creer(E_Parrainage p_parrainage)
    {
        _context.Parrainages.Add(p_parrainage);
        await _context.SaveChangesAsync();
    }

    public async Task Sauvegarder()
    {
        await _context.SaveChangesAsync();
    }

    // Apporteur correspondant à un code de parrainage déjà normalisé.
    // Un compte désactivé ne parraine plus : son code devient inopérant.
    public async Task<E_Utilisateur?> ObtenirApporteurParCode(string p_code)
    {
        return await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.CodeParrainage == p_code
                                   && u.Role == Enum_Role.ApporteurAffaire
                                   && u.EstActif);
    }

    // Plus grand numéro d'ordre déjà attribué pour un préfixe d'initiales.
    // Les codes partagent la forme « XY-nn » : on filtre sur le préfixe puis on
    // calcule le maximum côté serveur applicatif (volumes faibles, pas de SQL exotique).
    public async Task<List<string>> ObtenirCodesParPrefixe(string p_prefixe)
    {
        var _debut = p_prefixe + "-";
        return await _context.Utilisateurs
            .Where(u => u.CodeParrainage != null && u.CodeParrainage.StartsWith(_debut))
            .Select(u => u.CodeParrainage!)
            .ToListAsync();
    }

    // Prochain numéro libre pour un préfixe d'initiales : max(numéros existants) + 1.
    private async Task<string> ProchainCodeLibre(string p_prefixe)
    {
        var _codes = await ObtenirCodesParPrefixe(p_prefixe);
        var _maximum = 0;
        foreach (var _code in _codes)
        {
            var _numero = Shared.Helpers.H_CodeParrainage.ExtraireNumero(_code);
            if (_numero > _maximum)
            {
                _maximum = _numero;
            }
        }
        return Shared.Helpers.H_CodeParrainage.Composer(p_prefixe, _maximum + 1);
    }

    // Insère un apporteur en lui attribuant un code libre.
    // ⚠️ L'unicité est garantie par l'INDEX UNIQUE, pas par le calcul ci-dessus : deux
    // inscriptions simultanées aux mêmes initiales liraient le même maximum. En cas de
    // collision, on recalcule et on retente — l'entité n'est ajoutée au tracker qu'une
    // seule fois, seul le code change entre deux tentatives.
    // L'email est déjà vérifié comme libre en amont, donc un échec ici vient du code.
    public async Task<string?> CreerApporteur(E_Utilisateur p_utilisateur, string p_prefixe)
    {
        p_utilisateur.Email = p_utilisateur.Email.ToLower();
        p_utilisateur.DateCreation = DateTime.UtcNow;
        _context.Utilisateurs.Add(p_utilisateur);

        for (var _essai = 0; _essai < 5; _essai++)
        {
            p_utilisateur.CodeParrainage = await ProchainCodeLibre(p_prefixe);
            try
            {
                await _context.SaveChangesAsync();
                return p_utilisateur.CodeParrainage;
            }
            catch (DbUpdateException)
            {
                // Code pris entre-temps : on relit le maximum et on retente
            }
        }

        return null;
    }

    public async Task<E_Parrainage?> ObtenirParFilleul(int p_filleulId)
    {
        return await _context.Parrainages
            .Include(pa => pa.Apporteur)
            .FirstOrDefaultAsync(pa => pa.FilleulId == p_filleulId);
    }

    // Filleuls d'un apporteur, du plus récent au plus ancien
    public async Task<List<E_Parrainage>> ObtenirParApporteur(int p_apporteurId)
    {
        return await _context.Parrainages
            .Include(pa => pa.Filleul)
            .Where(pa => pa.ApporteurId == p_apporteurId)
            .OrderByDescending(pa => pa.DateInscription)
            .ToListAsync();
    }

    public async Task<List<E_Utilisateur>> ObtenirTousLesApporteurs()
    {
        return await _context.Utilisateurs
            .Where(u => u.Role == Enum_Role.ApporteurAffaire)
            .OrderBy(u => u.Nom)
            .ThenBy(u => u.Prenom)
            .ToListAsync();
    }

    // Compteurs et cumul d'un apporteur, en une seule lecture agrégée :
    // le tableau de bord est rafraîchi toutes les 60 s, pas question de recharger
    // la liste complète pour afficher trois chiffres.
    public async Task<(int Total, int EnAttente, decimal Cumule)> ObtenirStatistiques(int p_apporteurId)
    {
        var _lignes = await _context.Parrainages
            .Where(pa => pa.ApporteurId == p_apporteurId)
            .Select(pa => new { pa.Statut, pa.MontantCommission })
            .ToListAsync();

        var _total = _lignes.Count;
        var _enAttente = 0;
        decimal _cumule = 0m;

        foreach (var _l in _lignes)
        {
            if (_l.Statut == Enum_StatutParrainage.Valide)
            {
                _cumule += _l.MontantCommission;
            }
            else
            {
                _enAttente++;
            }
        }

        return (_total, _enAttente, _cumule);
    }
}
