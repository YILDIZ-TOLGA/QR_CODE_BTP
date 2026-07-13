using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.DTOs;

public class DTO_CreerCollaborateur
{
    public string Email { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public Enum_RoleEntreprise RoleEntreprise { get; set; } = Enum_RoleEntreprise.Collaborateur;
}
