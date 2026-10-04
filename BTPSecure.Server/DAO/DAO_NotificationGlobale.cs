using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

public class DAO_NotificationGlobale
{
    private readonly AppDbContext _context;

    public DAO_NotificationGlobale(AppDbContext p_context)
    {
        _context = p_context;
    }

    public async Task Creer(E_NotificationGlobale p_notification)
    {
        _context.NotificationsGlobales.Add(p_notification);
        await _context.SaveChangesAsync();
    }

    public async Task Sauvegarder()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<E_NotificationGlobale?> ObtenirParId(int p_id)
    {
        return await _context.NotificationsGlobales.FirstOrDefaultAsync(n => n.Id == p_id);
    }

    public async Task<List<E_NotificationGlobale>> ObtenirToutes()
    {
        return await _context.NotificationsGlobales
            .OrderByDescending(n => n.DateCreation)
            .ToListAsync();
    }

    // Diffusions actives à l'instant p_maintenant. Le filtrage par rôle se fait
    // ensuite en mémoire (volumes minuscules, et une recherche de sous-chaîne en
    // base sur une liste concaténée serait fragile).
    public async Task<List<E_NotificationGlobale>> ObtenirActives(DateTime p_maintenant)
    {
        return await _context.NotificationsGlobales
            .Where(n => n.EstActive && n.DateDebut <= p_maintenant && n.DateFin >= p_maintenant)
            .OrderBy(n => n.DateCreation)
            .ToListAsync();
    }

    // Identifiants des diffusions déjà vues par un utilisateur
    public async Task<List<int>> ObtenirIdsVues(int p_utilisateurId)
    {
        return await _context.NotificationsGlobalesVues
            .Where(v => v.UtilisateurId == p_utilisateurId)
            .Select(v => v.NotificationGlobaleId)
            .ToListAsync();
    }

    // Marque plusieurs diffusions comme vues.
    // ⚠️ L'index unique (diffusion, utilisateur) est le vrai garde-fou : deux onglets
    // ouverts simultanément peuvent déclencher deux insertions. On ignore l'échec
    // plutôt que de faire échouer l'affichage pour un doublon sans conséquence.
    public async Task MarquerVues(int p_utilisateurId, List<int> p_notificationIds)
    {
        if (p_notificationIds.Count == 0)
            return;

        foreach (var _id in p_notificationIds)
        {
            _context.NotificationsGlobalesVues.Add(new E_NotificationGlobaleVue
            {
                NotificationGlobaleId = _id,
                UtilisateurId = p_utilisateurId,
                DateVue = DateTime.UtcNow
            });
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Déjà vue : rien à faire. On vide le suivi pour ne pas laisser le contexte
            // dans un état qui ferait échouer la requête suivante.
            foreach (var _entree in _context.ChangeTracker.Entries<E_NotificationGlobaleVue>().ToList())
            {
                _entree.State = EntityState.Detached;
            }
        }
    }

    public async Task<int> CompterVues(int p_notificationId)
    {
        return await _context.NotificationsGlobalesVues.CountAsync(v => v.NotificationGlobaleId == p_notificationId);
    }

    // Combien de comptes actifs portent l'un de ces rôles
    public async Task<int> CompterDestinataires(List<Enum_Role> p_roles)
    {
        if (p_roles.Count == 0)
            return 0;

        return await _context.Utilisateurs.CountAsync(u => u.EstActif && p_roles.Contains(u.Role));
    }

    public async Task Supprimer(int p_id)
    {
        // Les traces de lecture partent en cascade (voir le mapping)
        await _context.NotificationsGlobales.Where(n => n.Id == p_id).ExecuteDeleteAsync();
    }
}
