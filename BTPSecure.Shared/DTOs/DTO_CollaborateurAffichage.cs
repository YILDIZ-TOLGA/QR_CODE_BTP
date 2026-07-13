namespace BTPSecure.Shared.DTOs;

using BTPSecure.Shared.Enums;

public class DTO_CollaborateurAffichage
{
    public int Id { get; set; }
    public int CollaborateurId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateAjout { get; set; }
    public Enum_StatutInvitation StatutInvitation { get; set; }
    public Enum_RoleEntreprise RoleEntreprise { get; set; }
}
