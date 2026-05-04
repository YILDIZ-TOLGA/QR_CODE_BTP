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
            .Include(c => c.Salarie)
            .Include(c => c.Entreprise)
            .FirstOrDefaultAsync(c => c.Id == p_id);
    }

    public async Task<E_Code?> ObtenirParValeur(string p_valeur)
    {
        return await _context.Codes
            .Include(c => c.Salarie)
            .Include(c => c.Entreprise)
            .FirstOrDefaultAsync(c => c.Valeur == p_valeur);
    }

    public async Task<List<E_Code>> ObtenirParPatron(int p_patronId)
    {
        var _codes = await _context.Codes
            .Include(c => c.Salarie)
            .Include(c => c.Entreprise)
            .Include(c => c.FournisseurContact)
            .Where(c => c.PatronId == p_patronId)
            .OrderByDescending(c => c.DateCreation)
            .ToListAsync();

        MettreAJourExpirations(_codes);
        await _context.SaveChangesAsync();
        return _codes;
    }

    public async Task<List<E_Code>> ObtenirCommandesPourFournisseur(string p_siret, string? p_siren)
    {
        var _maintenant = DateTime.UtcNow;
        var _codes = await _context.Codes
            .Include(c => c.FournisseurContact)
            .Where(c => c.FournisseurContact != null
                && c.Statut == Enum_StatutCode.Actif
                && c.FournisseurContact!.Siret == p_siret
                && (c.FournisseurContact.Siren == null || p_siren == null || c.FournisseurContact.Siren == p_siren)
                && (c.DateExpiration == null || c.DateExpiration > _maintenant))
            .OrderBy(c => c.DateExpiration)
            .ThenByDescending(c => c.DateCreation)
            .ToListAsync();

        MettreAJourExpirations(_codes);
        await _context.SaveChangesAsync();
        return _codes.Where(c => c.Statut == Enum_StatutCode.Actif).ToList();
    }

    public async Task<List<E_Code>> ObtenirParSalarie(int p_salarieId)
    {
        var _codes = await _context.Codes
            .Include(c => c.Entreprise)
            .Where(c => c.SalarieId == p_salarieId && c.Statut == Enum_StatutCode.Actif)
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

    public async Task RevoquerCodesParSalarieEtEntreprise(int p_salarieId, int p_entrepriseId)
    {
        var _codes = await _context.Codes
            .Where(c => c.SalarieId == p_salarieId
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
