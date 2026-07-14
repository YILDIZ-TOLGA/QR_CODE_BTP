namespace BTPSecure.Shared.Entites;

public class E_Ticket
{
    public int Id { get; set; }

    // Expéditeur : toujours un utilisateur connecté
    public int ExpediteurId { get; set; }
    public E_Utilisateur Expediteur { get; set; } = null!;

    // Destinataire interne (compte existant) OU email externe (pas de compte)
    public int? DestinataireId { get; set; }
    public E_Utilisateur? Destinataire { get; set; }
    public string? EmailDestinataire { get; set; }

    public string Sujet { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Pièce jointe stockée en base (bytea) car le disque Railway est éphémère
    public byte[]? PieceJointe { get; set; }
    public string? NomPieceJointe { get; set; }
    public string? TypePieceJointe { get; set; }

    public DateTime DateCreation { get; set; }
    public bool EstLu { get; set; }
}
