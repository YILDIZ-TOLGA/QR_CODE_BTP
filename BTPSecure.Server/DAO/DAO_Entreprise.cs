using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

public class DAO_Entreprise
{
    private readonly AppDbContext _context;

    public DAO_Entreprise(AppDbContext p_context)
    {
        _context = p_context;
    }

    public async Task<E_Entreprise?> ObtenirParPatronId(int p_patronId)
    {
        return await _context.Entreprises
            .FirstOrDefaultAsync(e => e.PatronId == p_patronId);
    }

    public async Task<E_Entreprise?> ObtenirParId(int p_id)
    {
        return await _context.Entreprises.FindAsync(p_id);
    }

    public async Task<E_Entreprise> Creer(E_Entreprise p_entreprise)
    {
        p_entreprise.DateCreation = DateTime.UtcNow;
        _context.Entreprises.Add(p_entreprise);
        await _context.SaveChangesAsync();
        return p_entreprise;
    }

    public async Task<E_SalarieEntreprise?> ObtenirLienSalarie(int p_salarieId, int p_entrepriseId)
    {
        return await _context.SalariesEntreprises
            .FirstOrDefaultAsync(se => se.SalarieId == p_salarieId
                && se.EntrepriseId == p_entrepriseId
                && se.EstActif);
    }

    public async Task<bool> SalarieEstDansEntreprise(int p_salarieId, int p_entrepriseId)
    {
        return await _context.SalariesEntreprises
            .AnyAsync(se => se.SalarieId == p_salarieId
                && se.EntrepriseId == p_entrepriseId
                && se.EstActif);
    }

    public async Task<E_SalarieEntreprise> AjouterSalarie(E_SalarieEntreprise p_lien)
    {
        p_lien.DateAjout = DateTime.UtcNow;
        p_lien.EstActif = true;
        _context.SalariesEntreprises.Add(p_lien);
        await _context.SaveChangesAsync();
        return p_lien;
    }

    public async Task<List<E_SalarieEntreprise>> ObtenirSalaries(int p_entrepriseId)
    {
        return await _context.SalariesEntreprises
            .Include(se => se.Salarie)
            .Where(se => se.EntrepriseId == p_entrepriseId && se.EstActif)
            .OrderBy(se => se.Salarie.Nom)
            .ToListAsync();
    }

    public async Task RetirerSalarie(E_SalarieEntreprise p_lien)
    {
        p_lien.EstActif = false;
        await _context.SaveChangesAsync();
    }
}
