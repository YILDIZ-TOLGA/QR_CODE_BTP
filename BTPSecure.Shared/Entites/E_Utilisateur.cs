using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.Entites;

public class E_Utilisateur
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string MotDePasseHash { get; set; } = string.Empty;
    public string Sel { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    // Nom de l'entreprise (fournisseurs) ; les sous-comptes l'héritent du compte principal
    public string? NomSociete { get; set; }
    public string? Telephone { get; set; }
    public string? Siret { get; set; }
    public string? Siren { get; set; }
    // Nombre maximum de sous-comptes autorisés (modifiable par l'admin)
    public int LimiteSousComptes { get; set; } = 3;
    public Enum_Role Role { get; set; }
    public DateTime DateCreation { get; set; }
    public int? ParentFournisseurId { get; set; }
    public bool EstActif { get; set; } = true;
    public bool EstValide { get; set; } = true;
    public bool EmailVerifie { get; set; }
    public string? TokenVerification { get; set; }
    public DateTime? TokenVerificationExpiration { get; set; }
}
