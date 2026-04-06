using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

public class DAO_Utilisateur
{
    private readonly AppDbContext _context;

    public DAO_Utilisateur(AppDbContext p_context)
    {
        _context = p_context;
    }

    public async Task<E_Utilisateur?> ObtenirParEmail(string p_email)
    {
        return await _context.Utilisateurs
            .FirstOrDefaultAsync(u => u.Email == p_email.ToLower());
    }

    public async Task<E_Utilisateur?> ObtenirParId(int p_id)
    {
        return await _context.Utilisateurs.FindAsync(p_id);
    }

    public async Task<bool> EmailExiste(string p_email)
    {
        return await _context.Utilisateurs
            .AnyAsync(u => u.Email == p_email.ToLower());
    }

    public async Task<E_Utilisateur> Creer(E_Utilisateur p_utilisateur)
    {
        p_utilisateur.Email = p_utilisateur.Email.ToLower();
        p_utilisateur.DateCreation = DateTime.UtcNow;
        _context.Utilisateurs.Add(p_utilisateur);
        await _context.SaveChangesAsync();
        return p_utilisateur;
    }
}
