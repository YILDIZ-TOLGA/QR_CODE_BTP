using System.Linq.Expressions;
using BTPSecure.Server.Data;
using BTPSecure.Shared.Entites;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.DAO;

// Projection légère d'un ticket SANS la pièce jointe (bytea) — pour les listes/fils.
// Évite de charger les octets des pièces jointes tant qu'on ne les télécharge pas.
public class TicketApercu
{
    public int Id { get; set; }
    public int ExpediteurId { get; set; }
    public string ExpediteurNom { get; set; } = string.Empty;
    public string ExpediteurPrenom { get; set; } = string.Empty;
    public string ExpediteurEmail { get; set; } = string.Empty;
    public int? DestinataireId { get; set; }
    public string? DestinataireNom { get; set; }
    public string? DestinatairePrenom { get; set; }
    public string? DestinataireEmail { get; set; }
    public string? EmailDestinataireExterne { get; set; }
    public string Sujet { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool ADocumentJoint { get; set; }
    public string? NomPieceJointe { get; set; }
    public string? TypePieceJointe { get; set; }
    public DateTime DateCreation { get; set; }
    public bool EstLu { get; set; }
}

public class DAO_Ticket
{
    private readonly AppDbContext _context;

    public DAO_Ticket(AppDbContext p_context)
    {
        _context = p_context;
    }

    // Projection réutilisée : présence de la pièce jointe par le nom (pas de bytea chargé)
    private static readonly Expression<Func<E_Ticket, TicketApercu>> _projApercu = t => new TicketApercu
    {
        Id = t.Id,
        ExpediteurId = t.ExpediteurId,
        ExpediteurNom = t.Expediteur.Nom,
        ExpediteurPrenom = t.Expediteur.Prenom,
        ExpediteurEmail = t.Expediteur.Email,
        DestinataireId = t.DestinataireId,
        DestinataireNom = t.Destinataire!.Nom,
        DestinatairePrenom = t.Destinataire!.Prenom,
        DestinataireEmail = t.Destinataire!.Email,
        EmailDestinataireExterne = t.EmailDestinataire,
        Sujet = t.Sujet,
        Message = t.Message,
        ADocumentJoint = t.NomPieceJointe != null,
        NomPieceJointe = t.NomPieceJointe,
        TypePieceJointe = t.TypePieceJointe,
        DateCreation = t.DateCreation,
        EstLu = t.EstLu
    };

    public async Task<E_Ticket> Creer(E_Ticket p_ticket)
    {
        p_ticket.DateCreation = DateTime.UtcNow;
        _context.Tickets.Add(p_ticket);
        await _context.SaveChangesAsync();
        return p_ticket;
    }

    // Tickets reçus (sans bytea)
    public async Task<List<TicketApercu>> ObtenirRecus(int p_userId)
    {
        return await _context.Tickets
            .Where(t => t.DestinataireId == p_userId)
            .OrderByDescending(t => t.DateCreation)
            .Select(_projApercu)
            .ToListAsync();
    }

    // Tickets envoyés (sans bytea)
    public async Task<List<TicketApercu>> ObtenirEnvoyes(int p_userId)
    {
        return await _context.Tickets
            .Where(t => t.ExpediteurId == p_userId)
            .OrderByDescending(t => t.DateCreation)
            .Select(_projApercu)
            .ToListAsync();
    }

    public async Task<int> CompterNonLus(int p_userId)
    {
        return await _context.Tickets
            .CountAsync(t => t.DestinataireId == p_userId && !t.EstLu);
    }

    // Une conversation existe-t-elle déjà entre ces deux utilisateurs ?
    public async Task<bool> ConversationExiste(int p_a, int p_b)
    {
        return await _context.Tickets
            .AnyAsync(t => (t.ExpediteurId == p_a && t.DestinataireId == p_b)
                        || (t.ExpediteurId == p_b && t.DestinataireId == p_a));
    }

    // Fil de conversation entre deux utilisateurs (sans bytea), du plus ancien au plus récent
    public async Task<List<TicketApercu>> ObtenirConversation(int p_a, int p_b)
    {
        return await _context.Tickets
            .Where(t => (t.ExpediteurId == p_a && t.DestinataireId == p_b)
                     || (t.ExpediteurId == p_b && t.DestinataireId == p_a))
            .OrderBy(t => t.DateCreation)
            .Select(_projApercu)
            .ToListAsync();
    }

    // Marque comme lus les messages reçus depuis un interlocuteur (sans charger les lignes)
    public async Task MarquerLusConversation(int p_userId, int p_autreId)
    {
        await _context.Tickets
            .Where(t => t.ExpediteurId == p_autreId && t.DestinataireId == p_userId && !t.EstLu)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.EstLu, true));
    }

    // Marque un ticket comme lu (uniquement si l'utilisateur en est le destinataire)
    public async Task MarquerLu(int p_ticketId, int p_userId)
    {
        await _context.Tickets
            .Where(t => t.Id == p_ticketId && t.DestinataireId == p_userId && !t.EstLu)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.EstLu, true));
    }

    // Chargement complet (avec bytea) — uniquement pour le téléchargement d'une pièce jointe
    public async Task<E_Ticket?> ObtenirParId(int p_id)
    {
        return await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == p_id);
    }

    // Suppression des tickets plus vieux que la limite (TTL 24 h)
    public async Task<int> SupprimerExpires(DateTime p_limite)
    {
        return await _context.Tickets
            .Where(t => t.DateCreation < p_limite)
            .ExecuteDeleteAsync();
    }
}
