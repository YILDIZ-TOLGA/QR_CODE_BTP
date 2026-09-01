using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

public class DAO_Notification
{
    private readonly AppDbContext _context;

    public DAO_Notification(AppDbContext p_context)
    {
        _context = p_context;
    }

    public async Task Creer(E_Notification p_notification)
    {
        p_notification.DateCreation = DateTime.UtcNow;
        _context.Notifications.Add(p_notification);
        await _context.SaveChangesAsync();
    }

    public async Task<List<E_Notification>> ObtenirNonLues(int p_utilisateurId)
    {
        return await _context.Notifications
            .Where(n => n.UtilisateurId == p_utilisateurId && !n.EstLue)
            .OrderBy(n => n.DateCreation)
            .ToListAsync();
    }

    // Marquage en masse sans charger les entités
    public async Task MarquerToutesLues(int p_utilisateurId)
    {
        await _context.Notifications
            .Where(n => n.UtilisateurId == p_utilisateurId && !n.EstLue)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.EstLue, true));
    }
}
