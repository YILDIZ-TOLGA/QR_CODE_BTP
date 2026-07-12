using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.DTOs;

public class DTO_InvitationAffichage
{
    public int Id { get; set; }
    public string NomEntreprise { get; set; } = string.Empty;
    public string? SiretEntreprise { get; set; }
    public string NomDirigeant { get; set; } = string.Empty;
    public string PrenomDirigeant { get; set; } = string.Empty;
    public DateTime DateAjout { get; set; }
    public Enum_StatutInvitation Statut { get; set; }
}
