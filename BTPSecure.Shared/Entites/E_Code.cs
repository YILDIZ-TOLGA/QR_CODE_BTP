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
    public int PatronId { get; set; }
    public E_Utilisateur Patron { get; set; } = null!;
    public int SalarieId { get; set; }
    public E_Utilisateur Salarie { get; set; } = null!;
    public int? FournisseurId { get; set; }
    public E_Utilisateur? Fournisseur { get; set; }
    public int EntrepriseId { get; set; }
    public E_Entreprise Entreprise { get; set; } = null!;
}
