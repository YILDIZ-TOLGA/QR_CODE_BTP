using System.Text;

namespace BTPSecure.Shared.Helpers;

// Code de parrainage d'un apporteur d'affaires : initiales + numéro d'ordre (ex. « TY-01 »).
// Partagé client + serveur : le serveur génère, le client valide la saisie avant l'appel réseau.
// ⚠️ Pas de string.Normalize(FormD) ici : le WASM client est trimmé et la globalisation
// peut y être réduite. La table d'équivalences ci-dessous est déterministe partout.
// ⚠️ Un code ne contient QUE des lettres ASCII A-Z : une table d'accents ne peut pas être
// exhaustive (ı turc, alphabets non latins…), donc tout caractère qui n'aboutit pas à
// une lettre A-Z est remplacé par « X » plutôt que recopié tel quel.
public static class H_CodeParrainage
{
    // ⚠️ Les deux tables sont indexées ensemble : elles DOIVENT rester de même longueur.
    private const string c_lettresAccentuees = "ÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝŸĞİıŞĆČŁŃŚŹŻ";
    private const string c_lettresSimples = "AAAAAACEEEEIIIINOOOOOUUUUYYGIISCCLNSZZ";

    // Caractère de remplacement quand aucune lettre exploitable n'est fournie
    private const char c_lettreParDefaut = 'X';

    // Numéro d'ordre maximal accepté à la saisie (garde-fou contre un collage aberrant)
    private const int c_numeroMaximal = 999999;

    // Deux initiales : prénom puis nom (Tolga YILDIZ → « TY »)
    public static string Initiales(string? p_nom, string? p_prenom)
    {
        var _initialePrenom = PremiereLettre(p_prenom);
        var _initialeNom = PremiereLettre(p_nom);
        return string.Concat(_initialePrenom, _initialeNom);
    }

    // Assemble un code à partir d'un préfixe et d'un numéro d'ordre.
    // Deux chiffres minimum, davantage au-delà de 99 (TY-01 … TY-99, TY-100).
    public static string Composer(string p_prefixe, int p_numero)
    {
        var _numero = p_numero.ToString();
        if (_numero.Length < 2)
        {
            _numero = _numero.PadLeft(2, '0');
        }
        return string.Concat(p_prefixe, "-", _numero);
    }

    // Saisie tolérante : « ty01 », « ty 01 », « TY-01 » donnent tous « TY-01 ».
    // Indispensable, le code circule à l'oral et par SMS.
    // Chaîne vide en retour = saisie inexploitable.
    public static string Normaliser(string? p_saisie)
    {
        if (string.IsNullOrWhiteSpace(p_saisie))
            return string.Empty;

        var _lettres = new StringBuilder();
        var _chiffres = new StringBuilder();

        foreach (var _c in p_saisie)
        {
            if (char.IsDigit(_c))
            {
                _chiffres.Append(_c);
                continue;
            }

            var _lettre = EnLettreAscii(_c);
            if (_lettre.HasValue)
            {
                // Une lettre après un chiffre : ce n'est pas la forme « préfixe puis numéro »
                if (_chiffres.Length > 0)
                    return string.Empty;
                _lettres.Append(_lettre.Value);
                continue;
            }

            // Séparateurs (espace, tiret, point) ignorés ; tout autre caractère invalide la saisie
            if (_c == '-' || _c == '_' || _c == '.' || char.IsWhiteSpace(_c))
                continue;

            return string.Empty;
        }

        if (_lettres.Length != 2)
            return string.Empty;
        if (_chiffres.Length == 0)
            return string.Empty;

        // Un numéro tout à zéro n'existe pas : la numérotation commence à 1
        var _valeur = _chiffres.ToString().TrimStart('0');
        if (_valeur.Length == 0)
            return string.Empty;

        int _numero;
        if (!int.TryParse(_valeur, out _numero))
            return string.Empty;
        if (_numero < 1 || _numero > c_numeroMaximal)
            return string.Empty;

        return Composer(_lettres.ToString(), _numero);
    }

    // Vrai si la saisie correspond à un code exploitable (sans garantir qu'il existe en base)
    public static bool EstFormatValide(string? p_saisie)
    {
        return Normaliser(p_saisie).Length > 0;
    }

    // Numéro d'ordre porté par un code déjà formé ; 0 si le code est illisible
    public static int ExtraireNumero(string? p_code)
    {
        var _normalise = Normaliser(p_code);
        if (_normalise.Length == 0)
            return 0;

        var _separateur = _normalise.IndexOf('-');
        if (_separateur < 0)
            return 0;

        int _numero;
        if (!int.TryParse(_normalise.Substring(_separateur + 1), out _numero))
            return 0;

        return _numero;
    }

    // Préfixe (les deux initiales) d'un code déjà formé ; chaîne vide si illisible
    public static string ExtrairePrefixe(string? p_code)
    {
        var _normalise = Normaliser(p_code);
        if (_normalise.Length == 0)
            return string.Empty;

        var _separateur = _normalise.IndexOf('-');
        if (_separateur < 0)
            return string.Empty;

        return _normalise.Substring(0, _separateur);
    }

    private static char PremiereLettre(string? p_texte)
    {
        if (string.IsNullOrWhiteSpace(p_texte))
            return c_lettreParDefaut;

        foreach (var _c in p_texte)
        {
            var _lettre = EnLettreAscii(_c);
            if (_lettre.HasValue)
                return _lettre.Value;
        }

        return c_lettreParDefaut;
    }

    // Convertit un caractère en lettre ASCII majuscule, accent retiré.
    // Null si le caractère n'aboutit pas à une lettre A-Z.
    private static char? EnLettreAscii(char p_c)
    {
        var _candidat = p_c;

        var _position = c_lettresAccentuees.IndexOf(char.ToUpperInvariant(p_c));
        if (_position < 0)
        {
            // Le « ı » turc n'a pas de majuscule ASCII : on le cherche aussi en minuscule
            _position = c_lettresAccentuees.IndexOf(p_c);
        }
        if (_position >= 0)
        {
            _candidat = c_lettresSimples[_position];
        }

        _candidat = char.ToUpperInvariant(_candidat);
        if (_candidat >= 'A' && _candidat <= 'Z')
            return _candidat;

        return null;
    }
}
