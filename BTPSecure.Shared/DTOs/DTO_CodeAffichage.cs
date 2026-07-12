using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.DTOs;

public class DTO_CodeAffichage
{
    public int Id { get; set; }
    public string Valeur { get; set; } = string.Empty;
    public Enum_TypeCode TypeCode { get; set; }
    public Enum_StatutCode Statut { get; set; }
    public string NumeroCommande { get; set; } = string.Empty;
    public string NomEntreprise { get; set; } = string.Empty;
    public string? Info { get; set; }
    public string? ListeMateriaux { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateExpiration { get; set; }
    public string NomCollaborateur { get; set; } = string.Empty;
    public string PrenomCollaborateur { get; set; } = string.Empty;
    public int CollaborateurId { get; set; }
    public string? Reference { get; set; }
    public string? NomFournisseurContact { get; set; }
    public int? FournisseurContactId { get; set; }
}
