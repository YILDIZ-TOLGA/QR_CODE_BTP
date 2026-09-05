namespace BTPSecure.Shared.DTOs;

// Résultat de la recherche d'une entreprise par SIRET ou SIREN (annuaire public de l'État)
public class DTO_InfoEntreprise
{
    public bool Trouve { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public string Siren { get; set; } = string.Empty;
    public string Siret { get; set; } = string.Empty;
    // Établissement fermé : on prévient l'utilisateur sans bloquer la saisie
    public bool EstActive { get; set; } = true;
}
