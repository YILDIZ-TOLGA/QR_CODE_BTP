namespace BTPSecure.Shared.DTOs;

public class DTO_FournisseurContact
{
    public int Id { get; set; }
    public string NomEntreprise { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Siret { get; set; } = string.Empty;
    public string? Siren { get; set; }
    public DateTime DateCreation { get; set; }
}

public class DTO_CreerFournisseurContact
{
    public string NomEntreprise { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Siret { get; set; } = string.Empty;
    public string? Siren { get; set; }
}
