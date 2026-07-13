namespace BTPSecure.Shared.DTOs;

public class DTO_ResultatValidation
{
    public bool EstValide { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? NomCollaborateur { get; set; }
    public string? PrenomCollaborateur { get; set; }
    public string? NumeroCommande { get; set; }
    public string? NomEntreprise { get; set; }
    public string? ListeMateriaux { get; set; }
    public string? Info { get; set; }
    public int AchatsSupplementaires { get; set; }
    public string? PdfBase64 { get; set; }
}
