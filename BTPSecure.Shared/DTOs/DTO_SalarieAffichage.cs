namespace BTPSecure.Shared.DTOs;

public class DTO_SalarieAffichage
{
    public int Id { get; set; }
    public int SalarieId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateAjout { get; set; }
}
