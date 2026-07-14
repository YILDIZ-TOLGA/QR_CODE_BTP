using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

public class DAO_Code
{
    private readonly AppDbContext _context;

    public DAO_Code(AppDbContext p_context)
    {
        _context = p_context;
    }

    public async Task<bool> ValeurExiste(string p_valeur)
    {
        return await _context.Codes.AnyAsync(c => c.Valeur == p_valeur);
    }

    public async Task<E_Code> Creer(E_Code p_code)
    {
        p_code.DateCreation = DateTime.UtcNow;
        p_code.Statut = Enum_StatutCode.Actif;
        _context.Codes.Add(p_code);
        await _context.SaveChangesAsync();
        return p_code;
    }

    public async Task<E_Code?> ObtenirParId(int p_id)
    {
        return await _context.Codes
            .Include(c => c.Collaborateur)
            .Include(c => c.Entreprise)
            .Include(c => c.Dirigeant)
            .FirstOrDefaultAsync(c => c.Id == p_id);
    }

    public async Task<E_Code?> ObtenirParValeur(string p_valeur)
    {
        return await _context.Codes
            .Include(c => c.Collaborateur)
            .Include(c => c.Entreprise)
            .Include(c => c.Dirigeant)
            .FirstOrDefaultAsync(c => c.Valeur == p_valeur);
    }

    public async Task<List<E_Code>> ObtenirValidationsParFournisseurs(List<int> p_fournisseurIds)
    {
        return await _context.Codes
            .Include(c => c.Fournisseur)
            .Include(c => c.Entreprise)
            .Where(c => c.FournisseurId != null
                && p_fournisseurIds.Contains(c.FournisseurId.Value)
                && c.DateValidation != null)
            .OrderByDescending(c => c.DateValidation)
            .ToListAsync();
    }

    public async Task<E_Code?> ObtenirCodePermanentActif(int p_collaborateurId, int p_entrepriseId)
    {
        return await _context.Codes
            .FirstOrDefaultAsync(c => c.CollaborateurId == p_collaborateurId
                && c.EntrepriseId == p_entrepriseId
                && c.EstPermanent
                && c.Statut == Enum_StatutCode.Actif);
    }

    public async Task<List<E_Code>> ObtenirParDirigeant(int p_dirigeantId)
    {
        var _codes = await _context.Codes
            .Include(c => c.Collaborateur)
            .Include(c => c.Entreprise)
            .Include(c => c.FournisseurContact)
            .Where(c => c.DirigeantId == p_dirigeantId)
            .OrderByDescending(c => c.DateCreation)
            .ToListAsync();

        MettreAJourExpirations(_codes);
        await _context.SaveChangesAsync();
        return _codes;
    }

    public async Task<List<E_Code>> ObtenirNotificationsPourDirigeant(int p_dirigeantId)
    {
        var _maintenant = DateTime.UtcNow;
        var _codes = await _context.Codes
            .Include(c => c.Collaborateur)
            .Include(c => c.FournisseurContact)
            .Where(c => c.DirigeantId == p_dirigeantId
                && c.EstPrete
                && c.Statut == Enum_StatutCode.Actif
                && (c.DateExpiration == null || c.DateExpiration > _maintenant))
            .OrderByDescending(c => c.DatePrete)
            .ToListAsync();

        MettreAJourExpirations(_codes);
        await _context.SaveChangesAsync();
        return _codes.Where(c => c.Statut == Enum_StatutCode.Actif).ToList();
    }

    public async Task<List<E_Code>> ObtenirCommandesPourFournisseur(string p_siret, string? p_siren)
    {
        var _maintenant = DateTime.UtcNow;
        var _codes = await _context.Codes
            .Include(c => c.FournisseurContact)
            .Include(c => c.Dirigeant)
            .Include(c => c.Collaborateur)
            .Where(c => c.FournisseurContact != null
                && c.Statut == Enum_StatutCode.Actif
                && c.FournisseurContact!.Siret == p_siret
                && ((c.FournisseurContact.Siren == null && p_siren == null)
                    || (c.FournisseurContact.Siren != null && p_siren != null && c.FournisseurContact.Siren == p_siren))
                && (c.DateExpiration == null || c.DateExpiration > _maintenant))
            .OrderBy(c => c.DateExpiration)
            .ThenByDescending(c => c.DateCreation)
            .ToListAsync();

        MettreAJourExpirations(_codes);
        await _context.SaveChangesAsync();
        return _codes.Where(c => c.Statut == Enum_StatutCode.Actif).ToList();
    }

    public async Task<List<E_Code>> ObtenirParCollaborateur(int p_collaborateurId)
    {
        var _codes = await _context.Codes
            .Include(c => c.Entreprise)
            .Where(c => c.CollaborateurId == p_collaborateurId && c.Statut == Enum_StatutCode.Actif)
            .OrderByDescending(c => c.DateCreation)
            .ToListAsync();

        MettreAJourExpirations(_codes);
        await _context.SaveChangesAsync();
        return _codes.Where(c => c.Statut == Enum_StatutCode.Actif).ToList();
    }

    public async Task Sauvegarder()
    {
        await _context.SaveChangesAsync();
    }

    public async Task RevoquerCodesParCollaborateurEtEntreprise(int p_collaborateurId, int p_entrepriseId)
    {
        var _codes = await _context.Codes
            .Where(c => c.CollaborateurId == p_collaborateurId
                && c.EntrepriseId == p_entrepriseId
                && c.Statut == Enum_StatutCode.Actif)
            .ToListAsync();

        foreach (var _code in _codes)
        {
            _code.Statut = Enum_StatutCode.Revoque;
        }

        await _context.SaveChangesAsync();
    }

    private void MettreAJourExpirations(List<E_Code> p_codes)
    {
        var _maintenant = DateTime.UtcNow;
        foreach (var _code in p_codes)
        {
            if (_code.Statut == Enum_StatutCode.Actif
                && _code.DateExpiration.HasValue
                && _code.DateExpiration.Value < _maintenant)
            {
                _code.Statut = Enum_StatutCode.Expire;
            }
        }
    }
}
