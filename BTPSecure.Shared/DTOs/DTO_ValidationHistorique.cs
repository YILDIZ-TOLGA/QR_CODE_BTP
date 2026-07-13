namespace BTPSecure.Shared.DTOs;

public class DTO_ValidationHistorique
{
    public int CodeId { get; set; }
    public string Valeur { get; set; } = string.Empty;
    public string NumeroCommande { get; set; } = string.Empty;
    public string NomEntreprise { get; set; } = string.Empty;
    public DateTime DateValidation { get; set; }
    public int ValidateurId { get; set; }
    public string NomValidateur { get; set; } = string.Empty;
    public int AchatsSupplementaires { get; set; }
}
