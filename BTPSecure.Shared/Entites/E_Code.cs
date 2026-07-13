using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.Entites;

public class E_Code
{
    public int Id { get; set; }
    public string Valeur { get; set; } = string.Empty;
    public Enum_TypeCode TypeCode { get; set; }
    public Enum_StatutCode Statut { get; set; }
    public string NumeroCommande { get; set; } = string.Empty;
    public string NomEntreprise { get; set; } = string.Empty;
    public string? Info { get; set; }
    public string? ListeMateriaux { get; set; }
    public int? DureeValidite { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateExpiration { get; set; }
    public DateTime? DateValidation { get; set; }
    public int DirigeantId { get; set; }
    public E_Utilisateur Dirigeant { get; set; } = null!;
    public int? CollaborateurId { get; set; }
    public E_Utilisateur? Collaborateur { get; set; }
    public string? EmailTiers { get; set; }
    public int AchatsSupplementaires { get; set; }
    public int? FournisseurId { get; set; }
    public E_Utilisateur? Fournisseur { get; set; }
    public int EntrepriseId { get; set; }
    public E_Entreprise Entreprise { get; set; } = null!;
    public string? Reference { get; set; }
    public int? FournisseurContactId { get; set; }
    public E_FournisseurContact? FournisseurContact { get; set; }
    public bool EstPrete { get; set; }
    public DateTime? DatePrete { get; set; }
    public bool EstPermanent { get; set; }
}
