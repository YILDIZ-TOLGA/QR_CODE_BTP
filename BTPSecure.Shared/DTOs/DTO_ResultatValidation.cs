namespace BTPSecure.Shared.DTOs;

public class DTO_ResultatValidation
{
    public bool EstValide { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? NomSalarie { get; set; }
    public string? PrenomSalarie { get; set; }
    public string? NumeroCommande { get; set; }
    public string? NomEntreprise { get; set; }
    public string? ListeMateriaux { get; set; }
    public string? PdfBase64 { get; set; }
}
