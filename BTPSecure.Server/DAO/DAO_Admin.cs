using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

public class DAO_Admin
{
    private readonly AppDbContext _context;

    public DAO_Admin(AppDbContext p_context)
    {
        _context = p_context;
    }

    public async Task<List<E_Entreprise>> ObtenirToutesLesEntreprises()
    {
        return await _context.Entreprises
            .Include(e => e.Patron)
            .OrderByDescending(e => e.DateCreation)
            .ToListAsync();
    }

    public async Task<E_Entreprise?> ObtenirEntrepriseParId(int p_id)
    {
        return await _context.Entreprises
            .Include(e => e.Patron)
            .FirstOrDefaultAsync(e => e.Id == p_id);
    }

    public async Task<int> CompterSalaries(int p_entrepriseId)
    {
        return await _context.SalariesEntreprises
            .CountAsync(se => se.EntrepriseId == p_entrepriseId && se.EstActif);
    }

    public async Task<int> CompterCodes(int p_entrepriseId)
    {
        return await _context.Codes
            .CountAsync(c => c.EntrepriseId == p_entrepriseId);
    }

    public async Task Sauvegarder()
    {
        await _context.SaveChangesAsync();
    }
}
