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
    // Code de parrainage d'un apporteur d'affaires, saisi par un DIRIGEANT a l'inscription.
    // Optionnel ; s'il est renseigne mais inconnu, l'inscription est refusee plutot
    // qu'ignoree silencieusement (sinon l'apporteur perdrait sa commission sans le savoir).
    public string? CodeParrainage { get; set; }
}
