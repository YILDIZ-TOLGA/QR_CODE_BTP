namespace BTPSecure.Shared.Enums;

// Cycle de vie d'un parrainage. La commission n'est fixée qu'au passage en Valide,
// par l'admin, au moment où il autorise l'entreprise du filleul.
public enum Enum_StatutParrainage
{
    EnAttente = 0,
    Valide = 1
}
