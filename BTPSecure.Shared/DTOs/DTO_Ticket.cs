namespace BTPSecure.Shared.DTOs;

// Envoi d'un nouveau ticket
public class DTO_EnvoyerTicket
{
    // Destinataire interne (choisi dans l'annuaire) OU email externe
    public int? DestinataireId { get; set; }
    public string? EmailDestinataire { get; set; }

    public string Sujet { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Pièce jointe optionnelle (byte[] sérialisé en base64 par System.Text.Json)
    public byte[]? PieceJointe { get; set; }
    public string? NomPieceJointe { get; set; }
    public string? TypePieceJointe { get; set; }
}

// Affichage d'un ticket (reçu ou envoyé) — sans les octets de la pièce jointe
public class DTO_Ticket
{
    public int Id { get; set; }
    public int ExpediteurId { get; set; }
    public string NomExpediteur { get; set; } = string.Empty;
    public string EmailExpediteur { get; set; } = string.Empty;
    public int? DestinataireId { get; set; }
    public string NomDestinataire { get; set; } = string.Empty;
    public string EmailDestinataire { get; set; } = string.Empty;
    public string Sujet { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool ADocumentJoint { get; set; }
    public string? NomPieceJointe { get; set; }
    public string? TypePieceJointe { get; set; }
    public DateTime DateCreation { get; set; }
    public bool EstLu { get; set; }
    public bool EstEnvoyeParMoi { get; set; }
}

// Contact de l'annuaire interne (destinataires possibles)
public class DTO_ContactAnnuaire
{
    public int UtilisateurId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Relation { get; set; } = string.Empty;
}

// Contenu d'une pièce jointe pour affichage/téléchargement côté client
public class DTO_PieceJointe
{
    public string Nom { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Base64 { get; set; } = string.Empty;
}
