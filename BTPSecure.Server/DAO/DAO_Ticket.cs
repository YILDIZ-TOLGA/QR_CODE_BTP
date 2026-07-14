using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

public class DAO_Ticket
{
    private readonly AppDbContext _context;

    public DAO_Ticket(AppDbContext p_context)
    {
        _context = p_context;
    }

    public async Task<E_Ticket> Creer(E_Ticket p_ticket)
    {
        p_ticket.DateCreation = DateTime.UtcNow;
        _context.Tickets.Add(p_ticket);
        await _context.SaveChangesAsync();
        return p_ticket;
    }

    // Tickets reçus par un utilisateur (destinataire interne)
    public async Task<List<E_Ticket>> ObtenirRecus(int p_userId)
    {
        return await _context.Tickets
            .Include(t => t.Expediteur)
            .Include(t => t.Destinataire)
            .Where(t => t.DestinataireId == p_userId)
            .OrderByDescending(t => t.DateCreation)
            .ToListAsync();
    }

    // Tickets envoyés par un utilisateur
    public async Task<List<E_Ticket>> ObtenirEnvoyes(int p_userId)
    {
        return await _context.Tickets
            .Include(t => t.Expediteur)
            .Include(t => t.Destinataire)
            .Where(t => t.ExpediteurId == p_userId)
            .OrderByDescending(t => t.DateCreation)
            .ToListAsync();
    }

    public async Task<int> CompterNonLus(int p_userId)
    {
        return await _context.Tickets
            .CountAsync(t => t.DestinataireId == p_userId && !t.EstLu);
    }

    // Fil de conversation entre deux utilisateurs (les deux sens), du plus ancien au plus récent
    public async Task<List<E_Ticket>> ObtenirConversation(int p_a, int p_b)
    {
        return await _context.Tickets
            .Include(t => t.Expediteur)
            .Include(t => t.Destinataire)
            .Where(t => (t.ExpediteurId == p_a && t.DestinataireId == p_b)
                     || (t.ExpediteurId == p_b && t.DestinataireId == p_a))
            .OrderBy(t => t.DateCreation)
            .ToListAsync();
    }

    public async Task<E_Ticket?> ObtenirParId(int p_id)
    {
        return await _context.Tickets
            .Include(t => t.Expediteur)
            .Include(t => t.Destinataire)
            .FirstOrDefaultAsync(t => t.Id == p_id);
    }

    public async Task Sauvegarder()
    {
        await _context.SaveChangesAsync();
    }

    // Suppression des tickets plus vieux que la limite (TTL 24 h)
    public async Task<int> SupprimerExpires(DateTime p_limite)
    {
        var _expires = await _context.Tickets
            .Where(t => t.DateCreation < p_limite)
            .ToListAsync();

        if (_expires.Count == 0)
            return 0;

        _context.Tickets.RemoveRange(_expires);
        await _context.SaveChangesAsync();
        return _expires.Count;
    }
}
