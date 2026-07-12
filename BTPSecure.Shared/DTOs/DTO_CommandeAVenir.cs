using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.DTOs;

public class DTO_CommandeAVenir
{
    public int CodeId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string NomEntrepriseDirigeant { get; set; } = string.Empty;
    public string NumeroCommande { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public DateTime? DateExpiration { get; set; }
    public string? ListeMateriaux { get; set; }
    public string? Info { get; set; }
    public Enum_TypeCode TypeCode { get; set; }
    public bool EstPrete { get; set; }
    public DateTime? DatePrete { get; set; }
}
