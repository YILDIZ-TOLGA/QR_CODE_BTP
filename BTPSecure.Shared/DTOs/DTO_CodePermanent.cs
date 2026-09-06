namespace BTPSecure.Shared.DTOs;

// Un porteur de code permanent, vu par le dirigeant
public class DTO_PorteurCodePermanent
{
    public int PorteurId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // Vide pour le dirigeant lui-même, qui n'a pas de rôle interne d'entreprise
    public string RoleLibelle { get; set; } = string.Empty;
    public bool EstDirigeant { get; set; }
    public DateTime DateCreationCode { get; set; }
    public int NombreUtilisations { get; set; }
    public DateTime? DerniereUtilisation { get; set; }
    // Cumul des achats supplémentaires autorisés sur toutes ses validations
    public int TotalAchatsSupplementaires { get; set; }
}

// Une utilisation du code, avec son validateur
public class DTO_UtilisationCode
{
    public int Id { get; set; }
    public DateTime DateValidation { get; set; }
    public string ValeurUtilisee { get; set; } = string.Empty;
    public string NumeroCommande { get; set; } = string.Empty;
    public int AchatsSupplementaires { get; set; }
    public bool EstPermanent { get; set; }
    // Fournisseur (ou sous-compte) qui a validé
    public string NomValidateur { get; set; } = string.Empty;
    public string SocieteValidateur { get; set; } = string.Empty;
    public string EmailValidateur { get; set; } = string.Empty;
}
