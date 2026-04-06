using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.DTOs;

public class DTO_ReponseAuth
{
    public string Token { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public Enum_Role Role { get; set; }
    public int UtilisateurId { get; set; }
}
