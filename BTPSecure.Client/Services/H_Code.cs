using System.Text;

namespace BTPSecure.Client.Services;

// Source UNIQUE du formatage de la saisie d'un code d'autorisation.
// Format : XXXX-XXXX (8 caractères alphanumériques + le séparateur).
public static class H_Code
{
    // Met en majuscules, retire tout caractère parasite (espaces, tirets saisis à la main)
    // et réinsère automatiquement le « - » après 4 caractères.
    public static string Formater(string? p_saisie)
    {
        if (string.IsNullOrEmpty(p_saisie))
        {
            return string.Empty;
        }

        var _propre = new StringBuilder();
        foreach (var _c in p_saisie)
        {
            if (char.IsLetterOrDigit(_c))
            {
                _propre.Append(char.ToUpperInvariant(_c));
            }
            if (_propre.Length == 8)
            {
                break;
            }
        }

        var _texte = _propre.ToString();
        if (_texte.Length <= 4)
        {
            return _texte;
        }
        return _texte.Substring(0, 4) + "-" + _texte.Substring(4);
    }
}
