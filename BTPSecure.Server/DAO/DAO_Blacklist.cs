using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

public class DAO_Blacklist
{
    private readonly AppDbContext _context;

    public DAO_Blacklist(AppDbContext p_context)
    {
        _context = p_context;
    }

    public async Task<List<E_Blacklist>> Lister(int p_fournisseurId)
    {
        return await _context.Blacklists
            .Where(b => b.FournisseurId == p_fournisseurId)
            .OrderByDescending(b => b.DateCreation)
            .ToListAsync();
    }

    public async Task<List<string>> ListerEmails(int p_fournisseurId)
    {
        return await _context.Blacklists
            .Where(b => b.FournisseurId == p_fournisseurId)
            .Select(b => b.Email)
            .ToListAsync();
    }

    public async Task<bool> Existe(int p_fournisseurId, string p_email)
    {
        return await _context.Blacklists
            .AnyAsync(b => b.FournisseurId == p_fournisseurId && b.Email == p_email.ToLower());
    }

    public async Task<E_Blacklist?> ObtenirParId(int p_id)
    {
        return await _context.Blacklists.FindAsync(p_id);
    }

    public async Task Ajouter(E_Blacklist p_b)
    {
        p_b.Email = p_b.Email.ToLower();
        p_b.DateCreation = DateTime.UtcNow;
        _context.Blacklists.Add(p_b);
        await _context.SaveChangesAsync();
    }

    public async Task Supprimer(E_Blacklist p_b)
    {
        _context.Blacklists.Remove(p_b);
        await _context.SaveChangesAsync();
    }
}
