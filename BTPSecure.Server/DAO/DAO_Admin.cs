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
            .Include(e => e.Dirigeant)
            .OrderByDescending(e => e.DateCreation)
            .ToListAsync();
    }

    public async Task<E_Entreprise?> ObtenirEntrepriseParId(int p_id)
    {
        return await _context.Entreprises
            .Include(e => e.Dirigeant)
            .FirstOrDefaultAsync(e => e.Id == p_id);
    }

    public async Task<int> CompterCollaborateurs(int p_entrepriseId)
    {
        return await _context.CollaborateursEntreprises
            .CountAsync(se => se.EntrepriseId == p_entrepriseId && se.EstActif);
    }

    // Responsables + Responsables Admin actifs (ils partagent le même plafond)
    public async Task<int> CompterResponsables(int p_entrepriseId)
    {
        return await _context.CollaborateursEntreprises
            .CountAsync(se => se.EntrepriseId == p_entrepriseId
                && se.EstActif
                && se.StatutInvitation == BTPSecure.Shared.Enums.Enum_StatutInvitation.Acceptee
                && (se.RoleEntreprise == BTPSecure.Shared.Enums.Enum_RoleEntreprise.Responsable
                    || se.RoleEntreprise == BTPSecure.Shared.Enums.Enum_RoleEntreprise.ResponsableAdmin));
    }

    public async Task<int> CompterCodes(int p_entrepriseId)
    {
        return await _context.Codes
            .CountAsync(c => c.EntrepriseId == p_entrepriseId);
    }

    public async Task<List<E_Utilisateur>> ObtenirFournisseurs()
    {
        return await _context.Utilisateurs
            .Where(u => u.Role == BTPSecure.Shared.Enums.Enum_Role.Fournisseur)
            .OrderByDescending(u => u.DateCreation)
            .ToListAsync();
    }

    public async Task<E_Utilisateur?> ObtenirUtilisateurParId(int p_id)
    {
        return await _context.Utilisateurs.FindAsync(p_id);
    }

    // Sous-comptes d'un fournisseur principal (blocage en cascade)
    public async Task<List<E_Utilisateur>> ObtenirSousComptes(int p_parentId)
    {
        return await _context.Utilisateurs
            .Where(u => u.ParentFournisseurId == p_parentId)
            .ToListAsync();
    }

    public async Task Sauvegarder()
    {
        await _context.SaveChangesAsync();
    }
}
