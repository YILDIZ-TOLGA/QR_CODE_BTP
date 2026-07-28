using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.DTOs;

public class DTO_Inscription
{
    public string Email { get; set; } = string.Empty;
    public string MotDePasse { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    // Obligatoire pour un fournisseur (nom/prénom optionnels dans ce cas)
    public string? NomSociete { get; set; }
    public string? Telephone { get; set; }
    public string? Siret { get; set; }
    public string? Siren { get; set; }
    public Enum_Role Role { get; set; }
}
