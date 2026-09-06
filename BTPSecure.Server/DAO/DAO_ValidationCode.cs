using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

public class DAO_ValidationCode
{
    private readonly AppDbContext _context;

    public DAO_ValidationCode(AppDbContext p_context)
    {
        _context = p_context;
    }

    public async Task Creer(E_ValidationCode p_validation)
    {
        _context.ValidationsCodes.Add(p_validation);
        await _context.SaveChangesAsync();
    }

    // Historique complet d'une entreprise, du plus récent au plus ancien
    public async Task<List<E_ValidationCode>> ObtenirParEntreprise(int p_entrepriseId)
    {
        return await _context.ValidationsCodes
            .Include(v => v.Porteur)
            .Include(v => v.Validateur)
            .Where(v => v.EntrepriseId == p_entrepriseId)
            .OrderByDescending(v => v.DateValidation)
            .ToListAsync();
    }

    // Historique d'un porteur précis, borné à son entreprise
    public async Task<List<E_ValidationCode>> ObtenirParPorteur(int p_entrepriseId, int p_porteurId)
    {
        return await _context.ValidationsCodes
            .Include(v => v.Porteur)
            .Include(v => v.Validateur)
            .Where(v => v.EntrepriseId == p_entrepriseId && v.PorteurId == p_porteurId)
            .OrderByDescending(v => v.DateValidation)
            .ToListAsync();
    }
}
