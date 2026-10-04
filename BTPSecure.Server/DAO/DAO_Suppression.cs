using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

// Suppression définitive d'un compte utilisateur par un administrateur.
// ⚠️ Toutes les clés étrangères du schéma sont en `Restrict` : c'est voulu (les traces
// d'argent et d'audit ne doivent pas s'évaporer avec un compte). Une suppression doit
// donc effacer les dépendances DANS LE BON ORDRE, et refuser net quand le compte est
// ancré dans des données qui ne lui appartiennent pas.
public class DAO_Suppression
{
    private readonly AppDbContext _context;

    public DAO_Suppression(AppDbContext p_context)
    {
        _context = p_context;
    }

    // ---------- Comptages pour l'aperçu ----------

    public async Task<int> CompterValidationsCommeValidateur(int p_id)
    {
        return await _context.ValidationsCodes.CountAsync(v => v.ValidateurId == p_id);
    }

    public async Task<int> CompterValidationsCommePorteur(int p_id)
    {
        return await _context.ValidationsCodes.CountAsync(v => v.PorteurId == p_id);
    }

    public async Task<int> CompterValidationsDeLEntreprise(int p_entrepriseId)
    {
        return await _context.ValidationsCodes.CountAsync(v => v.EntrepriseId == p_entrepriseId);
    }

    public async Task<int> CompterParrainagesValides(int p_id)
    {
        return await _context.Parrainages.CountAsync(pa =>
            (pa.ApporteurId == p_id || pa.FilleulId == p_id)
            && pa.Statut == Enum_StatutParrainage.Valide);
    }

    public async Task<int> CompterParrainagesEnAttente(int p_id)
    {
        return await _context.Parrainages.CountAsync(pa =>
            (pa.ApporteurId == p_id || pa.FilleulId == p_id)
            && pa.Statut == Enum_StatutParrainage.EnAttente);
    }

    public async Task<int> CompterSousComptes(int p_id)
    {
        return await _context.Utilisateurs.CountAsync(u => u.ParentFournisseurId == p_id);
    }

    public async Task<int> CompterCodesLies(int p_id)
    {
        return await _context.Codes.CountAsync(c =>
            c.DirigeantId == p_id || c.CollaborateurId == p_id);
    }

    public async Task<int> CompterMessages(int p_id)
    {
        return await _context.Tickets.CountAsync(t => t.ExpediteurId == p_id || t.DestinataireId == p_id);
    }

    public async Task<int> CompterMemos(int p_id)
    {
        return await _context.Memos.CountAsync(m => m.UtilisateurId == p_id);
    }

    public async Task<int> CompterNotifications(int p_id)
    {
        return await _context.Notifications.CountAsync(n => n.UtilisateurId == p_id);
    }

    public async Task<int> CompterLiensEntreprise(int p_id)
    {
        return await _context.CollaborateursEntreprises.CountAsync(l => l.CollaborateurId == p_id);
    }

    public async Task<int> CompterContactsFournisseur(int p_id)
    {
        return await _context.FournisseursContacts.CountAsync(f => f.DirigeantId == p_id);
    }

    public async Task<int> CompterBlacklist(int p_id)
    {
        return await _context.Blacklists.CountAsync(b => b.FournisseurId == p_id);
    }

    public async Task<E_Entreprise?> ObtenirEntrepriseDuDirigeant(int p_id)
    {
        return await _context.Entreprises.FirstOrDefaultAsync(e => e.DirigeantId == p_id);
    }

    // ---------- Suppression ----------

    // Efface le compte et ses dépendances, dans une TRANSACTION unique.
    // L'ordre suit les dépendances ; la transaction garantit qu'un oubli de ma part
    // se traduit par un refus complet et non par un compte à moitié supprimé.
    // Renvoie null si tout s'est bien passé, sinon le message de l'erreur rencontrée.
    public async Task<string?> Supprimer(int p_id)
    {
        await using var _transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1) Données strictement personnelles
            await _context.Notifications.Where(n => n.UtilisateurId == p_id).ExecuteDeleteAsync();
            await _context.Memos.Where(m => m.UtilisateurId == p_id).ExecuteDeleteAsync();
            await _context.ResetsMotDePasse.Where(r => r.UtilisateurId == p_id).ExecuteDeleteAsync();
            await _context.Tickets.Where(t => t.ExpediteurId == p_id || t.DestinataireId == p_id).ExecuteDeleteAsync();
            await _context.Blacklists.Where(b => b.FournisseurId == p_id).ExecuteDeleteAsync();

            // 2) Parrainages : seuls les « en attente » subsistent ici (les validés bloquent
            //    la suppression en amont), ils ne portent donc aucune trace d'argent.
            await _context.Parrainages
                .Where(pa => pa.ApporteurId == p_id || pa.FilleulId == p_id)
                .ExecuteDeleteAsync();

            // 2 bis) Parrainage qu'il aurait VALIDÉ. Aujourd'hui inatteignable : seul un
            //        admin valide, et les admins ne sont pas supprimables. On détache quand
            //        même, car la clé est en `Restrict` : si un jour un autre rôle pouvait
            //        valider, la suppression échouerait sans raison lisible. La colonne
            //        étant nullable, détacher suffit — la trace de la commission reste.
            await _context.Parrainages
                .Where(pa => pa.ValidateurId == p_id)
                .ExecuteUpdateAsync(s => s.SetProperty(pa => pa.ValidateurId, (int?)null));

            // 2 ter) Diffusions globales : sa trace de lecture part, et s'il en a redige
            //        une (admin), on la detache pour que le message survive au compte.
            await _context.NotificationsGlobalesVues.Where(v => v.UtilisateurId == p_id).ExecuteDeleteAsync();
            await _context.NotificationsGlobales
                .Where(n => n.CreateurId == p_id)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.CreateurId, (int?)null));

            // 3) Codes d'AUTRES entreprises où ce compte n'est qu'un intervenant :
            //    on détache sans détruire — un code vivant ne doit pas disparaître
            //    parce que son auteur ou son fournisseur est supprimé.
            await _context.Codes
                .Where(c => c.CreateurId == p_id)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.CreateurId, (int?)null));
            await _context.Codes
                .Where(c => c.FournisseurId == p_id)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.FournisseurId, (int?)null));

            // 4) Codes qui lui étaient DESTINÉS : sans destinataire ils n'ont plus d'objet.
            //    Aucun n'a été validé (garantie des blocages), donc rien d'historique ne part.
            await _context.Codes.Where(c => c.CollaborateurId == p_id).ExecuteDeleteAsync();

            // 5) Ses rattachements à des entreprises
            await _context.CollaborateursEntreprises.Where(l => l.CollaborateurId == p_id).ExecuteDeleteAsync();

            // 6) S'il est dirigeant, son entreprise disparaît avec lui
            var _entreprise = await _context.Entreprises.FirstOrDefaultAsync(e => e.DirigeantId == p_id);
            if (_entreprise != null)
            {
                await _context.Codes.Where(c => c.EntrepriseId == _entreprise.Id).ExecuteDeleteAsync();
                await _context.CollaborateursEntreprises.Where(l => l.EntrepriseId == _entreprise.Id).ExecuteDeleteAsync();
            }

            // 7) Filet : un code pointant encore ce compte comme dirigeant empêcherait
            //    la suppression finale (clé étrangère non nullable).
            await _context.Codes.Where(c => c.DirigeantId == p_id).ExecuteDeleteAsync();

            // 8) Carnet fournisseur : après les codes, qui peuvent le référencer
            await _context.FournisseursContacts.Where(f => f.DirigeantId == p_id).ExecuteDeleteAsync();

            if (_entreprise != null)
            {
                await _context.Entreprises.Where(e => e.Id == _entreprise.Id).ExecuteDeleteAsync();
            }

            // 9) Le compte
            var _lignes = await _context.Utilisateurs.Where(u => u.Id == p_id).ExecuteDeleteAsync();
            if (_lignes == 0)
            {
                await _transaction.RollbackAsync();
                return "Le compte n'existe plus.";
            }

            await _transaction.CommitAsync();
            return null;
        }
        catch (Exception ex)
        {
            await _transaction.RollbackAsync();
            // Message de la cause racine : une violation de clé étrangère est bien plus
            // parlante que l'enveloppe DbUpdateException.
            var _cause = ex;
            while (_cause.InnerException != null)
            {
                _cause = _cause.InnerException;
            }
            return _cause.Message;
        }
    }
}
