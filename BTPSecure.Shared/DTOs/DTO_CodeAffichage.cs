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
    public string NomSalarie { get; set; } = string.Empty;
    public string PrenomSalarie { get; set; } = string.Empty;
}
