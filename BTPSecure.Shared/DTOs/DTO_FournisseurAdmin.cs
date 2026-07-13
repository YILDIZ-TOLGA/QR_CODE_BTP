namespace BTPSecure.Shared.DTOs;

public class DTO_FournisseurAdmin
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Siret { get; set; }
    public string? Siren { get; set; }
    public string? Telephone { get; set; }
    public DateTime DateCreation { get; set; }
    public bool EstValide { get; set; }
    public bool EstActif { get; set; }
}
