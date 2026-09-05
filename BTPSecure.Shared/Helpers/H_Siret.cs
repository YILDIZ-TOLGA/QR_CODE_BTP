namespace BTPSecure.Shared.Helpers;

// Validation hors ligne des numéros SIREN (9 chiffres) et SIRET (14 chiffres).
// Partagé client + serveur : une seule implémentation, pas de divergence possible.
// Permet de rejeter une saisie erronée sans aucun appel réseau.
public static class H_Siret
{
    // La Poste ne respecte pas la clé de Luhn : c'est l'exception historique connue.
    private const string SIREN_LA_POSTE = "356000000";

    public static string GarderChiffres(string? p_valeur)
    {
        if (string.IsNullOrWhiteSpace(p_valeur))
            return string.Empty;
        return new string(p_valeur.Where(char.IsDigit).ToArray());
    }

    public static bool EstSirenValide(string? p_siren)
    {
        var _chiffres = GarderChiffres(p_siren);
        if (_chiffres.Length != 9)
            return false;
        return VerifierLuhn(_chiffres);
    }

    public static bool EstSiretValide(string? p_siret)
    {
        var _chiffres = GarderChiffres(p_siret);
        if (_chiffres.Length != 14)
            return false;
        if (_chiffres.StartsWith(SIREN_LA_POSTE))
            return true;
        return VerifierLuhn(_chiffres);
    }

    // Clé de contrôle : on double un chiffre sur deux en partant de la droite
    private static bool VerifierLuhn(string p_chiffres)
    {
        int _somme = 0;
        bool _doubler = false;

        for (int i = p_chiffres.Length - 1; i >= 0; i--)
        {
            int _valeur = p_chiffres[i] - '0';

            if (_doubler)
            {
                _valeur = _valeur * 2;
                if (_valeur > 9)
                {
                    _valeur = _valeur - 9;
                }
            }

            _somme = _somme + _valeur;
            _doubler = !_doubler;
        }

        return _somme % 10 == 0;
    }
}
