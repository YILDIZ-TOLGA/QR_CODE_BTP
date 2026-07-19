namespace BTPSecure.Shared.DTOs;

// Réattribution d'un code déjà généré (destinataire et/ou fournisseur).
// Changer le destinataire régénère la valeur du code : l'ancien porteur ne doit
// plus pouvoir l'utiliser.
public class DTO_ModifierCode
{
    public int CodeId { get; set; }

    // Destinataire : un collaborateur de l'entreprise OU un tiers externe
    public bool PourTiers { get; set; } = false;
    public int CollaborateurId { get; set; }
    public string? EmailTiers { get; set; }

    // Fournisseur désigné (issu du carnet) ; false = aucun fournisseur
    public bool UtiliserFournisseur { get; set; } = false;
    public int? FournisseurContactId { get; set; }
}

// Résultat de la modification, pour informer clairement l'utilisateur
public class DTO_ResultatModificationCode
{
    public string Message { get; set; } = string.Empty;
    public bool CodeRegenere { get; set; }
    public string NouvelleValeur { get; set; } = string.Empty;
}
