using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

public class DAO_Entreprise
{
    private readonly AppDbContext _context;

    public DAO_Entreprise(AppDbContext p_context)
    {
        _context = p_context;
    }

    public async Task<E_Entreprise?> ObtenirParDirigeantId(int p_dirigeantId)
    {
        return await _context.Entreprises
            .FirstOrDefaultAsync(e => e.DirigeantId == p_dirigeantId);
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

    public async Task<E_CollaborateurEntreprise?> ObtenirPremierLienResponsableAdmin(int p_collaborateurId)
    {
        return await _context.CollaborateursEntreprises
            .Include(se => se.Entreprise)
            .FirstOrDefaultAsync(se => se.CollaborateurId == p_collaborateurId
                && se.EstActif
                && se.StatutInvitation == Enum_StatutInvitation.Acceptee
                && se.RoleEntreprise == Enum_RoleEntreprise.ResponsableAdmin);
    }

    public async Task<E_CollaborateurEntreprise?> ObtenirLienCollaborateur(int p_collaborateurId, int p_entrepriseId)
    {
        return await _context.CollaborateursEntreprises
            .FirstOrDefaultAsync(se => se.CollaborateurId == p_collaborateurId
                && se.EntrepriseId == p_entrepriseId
                && se.EstActif);
    }

    public async Task<bool> CollaborateurEstDansEntreprise(int p_collaborateurId, int p_entrepriseId)
    {
        return await _context.CollaborateursEntreprises
            .AnyAsync(se => se.CollaborateurId == p_collaborateurId
                && se.EntrepriseId == p_entrepriseId
                && se.EstActif
                && se.StatutInvitation == Enum_StatutInvitation.Acceptee);
    }

    public async Task<bool> InvitationExiste(int p_collaborateurId, int p_entrepriseId)
    {
        return await _context.CollaborateursEntreprises
            .AnyAsync(se => se.CollaborateurId == p_collaborateurId
                && se.EntrepriseId == p_entrepriseId
                && se.EstActif
                && se.StatutInvitation == Enum_StatutInvitation.EnAttente);
    }

    public async Task<E_CollaborateurEntreprise> AjouterCollaborateur(E_CollaborateurEntreprise p_lien)
    {
        p_lien.DateAjout = DateTime.UtcNow;
        p_lien.EstActif = true;
        p_lien.StatutInvitation = Enum_StatutInvitation.EnAttente;
        _context.CollaborateursEntreprises.Add(p_lien);
        await _context.SaveChangesAsync();
        return p_lien;
    }

    public async Task<List<E_CollaborateurEntreprise>> ObtenirCollaborateurs(int p_entrepriseId)
    {
        return await _context.CollaborateursEntreprises
            .Include(se => se.Collaborateur)
            .Where(se => se.EntrepriseId == p_entrepriseId && se.EstActif)
            .OrderBy(se => se.StatutInvitation)
            .ThenBy(se => se.Collaborateur.Nom)
            .ToListAsync();
    }

    // Responsables + Responsables Admin d'une entreprise : ils partagent le même plafond.
    // Seuls les liens actifs dont l'invitation est acceptée occupent une place.
    public async Task<int> CompterResponsables(int p_entrepriseId)
    {
        return await _context.CollaborateursEntreprises
            .CountAsync(se => se.EntrepriseId == p_entrepriseId
                && se.EstActif
                && se.StatutInvitation == Enum_StatutInvitation.Acceptee
                && (se.RoleEntreprise == Enum_RoleEntreprise.Responsable
                    || se.RoleEntreprise == Enum_RoleEntreprise.ResponsableAdmin));
    }

    public async Task<List<E_CollaborateurEntreprise>> ObtenirInvitationsParCollaborateur(int p_collaborateurId)
    {
        return await _context.CollaborateursEntreprises
            .Include(se => se.Entreprise)
                .ThenInclude(e => e.Dirigeant)
            .Where(se => se.CollaborateurId == p_collaborateurId && se.EstActif)
            .OrderBy(se => se.StatutInvitation)
            .ThenByDescending(se => se.DateAjout)
            .ToListAsync();
    }

    public async Task<E_CollaborateurEntreprise?> ObtenirLienParId(int p_id)
    {
        return await _context.CollaborateursEntreprises
            .Include(se => se.Entreprise)
                .ThenInclude(e => e.Dirigeant)
            .FirstOrDefaultAsync(se => se.Id == p_id);
    }

    public async Task RetirerCollaborateur(E_CollaborateurEntreprise p_lien)
    {
        p_lien.EstActif = false;
        await _context.SaveChangesAsync();
    }

    public async Task Sauvegarder()
    {
        await _context.SaveChangesAsync();
    }
}
