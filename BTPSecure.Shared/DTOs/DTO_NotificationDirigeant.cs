namespace BTPSecure.Shared.DTOs;

public class DTO_NotificationDirigeant
{
    public int CodeId { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string NumeroCommande { get; set; } = string.Empty;
    public string NomFournisseur { get; set; } = string.Empty;
    public string EmailFournisseur { get; set; } = string.Empty;
    public string NomCollaborateur { get; set; } = string.Empty;
    public string PrenomCollaborateur { get; set; } = string.Empty;
    public DateTime DatePrete { get; set; }
    public DateTime? DateExpiration { get; set; }
    public string? ListeMateriaux { get; set; }
    public string? Info { get; set; }
}
