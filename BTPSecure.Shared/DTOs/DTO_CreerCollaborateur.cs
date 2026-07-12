namespace BTPSecure.Shared.DTOs;

public class DTO_CreerCollaborateur
{
    public string Email { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? Telephone { get; set; }
}
