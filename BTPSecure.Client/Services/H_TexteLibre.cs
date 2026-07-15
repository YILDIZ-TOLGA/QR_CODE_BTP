namespace BTPSecure.Client.Services;

// Source UNIQUE pour l'affichage des textes saisis par l'utilisateur
// (liste de matériaux, informations complémentaires...).
// - préserve les sauts de ligne
// - coupe les suites de caractères sans espace, sinon la carte déborde horizontalement
// - tronque au-delà d'un seuil, le détail complet s'ouvrant dans un dialogue
public static class H_TexteLibre
{
    public const int Seuil = 140;

    private const string _style = "white-space: pre-wrap; overflow-wrap: anywhere;";

    public static bool EstTronque(string? p_texte)
    {
        if (string.IsNullOrEmpty(p_texte))
        {
            return false;
        }
        return p_texte.Length > Seuil;
    }

    public static string Tronquer(string? p_texte)
    {
        if (string.IsNullOrEmpty(p_texte))
        {
            return string.Empty;
        }
        if (p_texte.Length <= Seuil)
        {
            return p_texte;
        }
        return p_texte.Substring(0, Seuil) + "…";
    }

    // Curseur main uniquement quand le texte est tronqué (donc cliquable)
    public static string Style(string? p_texte)
    {
        if (EstTronque(p_texte))
        {
            return _style + " cursor: pointer;";
        }
        return _style;
    }
}
