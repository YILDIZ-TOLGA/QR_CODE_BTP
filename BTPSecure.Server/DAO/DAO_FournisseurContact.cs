using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

public class DAO_FournisseurContact
{
    private readonly AppDbContext _context;

    public DAO_FournisseurContact(AppDbContext p_context)
    {
        _context = p_context;
    }

    public async Task<List<E_FournisseurContact>> ObtenirParPatron(int p_patronId)
    {
        return await _context.FournisseursContacts
            .Where(f => f.PatronId == p_patronId)
            .OrderBy(f => f.NomEntreprise)
            .ToListAsync();
    }

    public async Task<E_FournisseurContact?> ObtenirParId(int p_id)
    {
        return await _context.FournisseursContacts.FindAsync(p_id);
    }

    public async Task<E_FournisseurContact?> ObtenirParPatronEtSiret(int p_patronId, string p_siret)
    {
        return await _context.FournisseursContacts
            .FirstOrDefaultAsync(f => f.PatronId == p_patronId && f.Siret == p_siret);
    }

    public async Task<E_FournisseurContact> Creer(E_FournisseurContact p_contact)
    {
        p_contact.DateCreation = DateTime.UtcNow;
        _context.FournisseursContacts.Add(p_contact);
        await _context.SaveChangesAsync();
        return p_contact;
    }

    public async Task Sauvegarder()
    {
        await _context.SaveChangesAsync();
    }

    public async Task Supprimer(E_FournisseurContact p_contact)
    {
        _context.FournisseursContacts.Remove(p_contact);
        await _context.SaveChangesAsync();
    }
}
