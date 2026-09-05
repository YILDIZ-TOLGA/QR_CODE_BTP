using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.DTOs;

public class DTO_Profil
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public string? Siret { get; set; }
    public string? Siren { get; set; }
    public Enum_Role Role { get; set; }
    public DateTime DateCreation { get; set; }
    public List<string> Entreprises { get; set; } = new();
    // Responsable ou Responsable Admin dans au moins une entreprise.
    // Le JWT ne porte que « Collaborateur » : le client ne peut pas le déduire seul.
    public bool EstResponsable { get; set; }
}
