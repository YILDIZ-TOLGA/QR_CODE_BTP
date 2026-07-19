using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

public class DAO_Memo
{
    private readonly AppDbContext _context;

    public DAO_Memo(AppDbContext p_context)
    {
        _context = p_context;
    }

    // Mémos d'un utilisateur, le plus récemment modifié en premier
    public async Task<List<E_Memo>> Lister(int p_utilisateurId)
    {
        return await _context.Memos
            .Where(m => m.UtilisateurId == p_utilisateurId)
            .OrderByDescending(m => m.DateModification)
            .ToListAsync();
    }

    public async Task<E_Memo?> ObtenirParId(int p_id)
    {
        return await _context.Memos.FindAsync(p_id);
    }

    public async Task<E_Memo> Creer(E_Memo p_memo)
    {
        var _maintenant = DateTime.UtcNow;
        p_memo.DateCreation = _maintenant;
        p_memo.DateModification = _maintenant;
        _context.Memos.Add(p_memo);
        await _context.SaveChangesAsync();
        return p_memo;
    }

    public async Task Sauvegarder()
    {
        await _context.SaveChangesAsync();
    }

    public async Task Supprimer(E_Memo p_memo)
    {
        _context.Memos.Remove(p_memo);
        await _context.SaveChangesAsync();
    }
}
