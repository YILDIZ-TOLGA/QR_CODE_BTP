using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.DTOs;

public class DTO_Inscription
{
    public string Email { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string? Telephone { get; set; }
    public Enum_Role Role { get; set; }
}
