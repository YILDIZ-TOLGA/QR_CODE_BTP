using System.Collections.Concurrent;
using System.Text.Json;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Helpers;

namespace BTPSecure.Server.Services;

// Recherche d'une entreprise par SIRET / SIREN via l'annuaire public de l'État
// (recherche-entreprises.api.gouv.fr : gratuit, sans clé d'API).
// L'appel passe par le serveur et non par le navigateur : pas de CORS, et surtout
// un cache mutualisé qui évite de marteler l'API depuis l'IP unique de Railway.
public class S_RechercheEntreprise
{
    private const string URL_BASE = "https://recherche-entreprises.api.gouv.fr/search";
    private static readonly TimeSpan _dureeCache = TimeSpan.FromHours(24);

    private static readonly ConcurrentDictionary<string, Entree> _cache = new();

    private readonly HttpClient _http;
    private readonly ILogger<S_RechercheEntreprise> _logger;

    public S_RechercheEntreprise(HttpClient p_http, ILogger<S_RechercheEntreprise> p_logger)
    {
        _http = p_http;
        _logger = p_logger;
    }

    public async Task<DTO_InfoEntreprise> Rechercher(string p_identifiant)
    {
        var _chiffres = H_Siret.GarderChiffres(p_identifiant);

        // Contrôle hors ligne d'abord : inutile d'appeler l'API pour une saisie erronée
        bool _estSiret = _chiffres.Length == 14;
        bool _estSiren = _chiffres.Length == 9;

        if (!_estSiret && !_estSiren)
            return Echec("Saisissez un SIRET (14 chiffres) ou un SIREN (9 chiffres).");

        if (_estSiret && !H_Siret.EstSiretValide(_chiffres))
            return Echec("Ce SIRET est invalide (clé de contrôle incorrecte).");

        if (_estSiren && !H_Siret.EstSirenValide(_chiffres))
            return Echec("Ce SIREN est invalide (clé de contrôle incorrecte).");

        Entree _entree;
        if (_cache.TryGetValue(_chiffres, out _entree))
        {
            if (DateTime.UtcNow < _entree.Expiration)
                return _entree.Resultat;
            _cache.TryRemove(_chiffres, out _);
        }

        DTO_InfoEntreprise _resultat;
        try
        {
            _resultat = await Interroger(_chiffres, _estSiret);
        }
        catch (Exception ex)
        {
            // L'annuaire est un confort : son indisponibilité ne doit jamais bloquer une saisie
            _logger.LogWarning(ex, "Annuaire des entreprises injoignable pour {Identifiant}", _chiffres);
            return Echec("Annuaire indisponible pour le moment. Saisissez le nom à la main.");
        }

        // Seuls les résultats aboutis sont mis en cache : une panne ne doit pas se figer
        if (_resultat.Trouve)
        {
            var _nouvelle = new Entree();
            _nouvelle.Resultat = _resultat;
            _nouvelle.Expiration = DateTime.UtcNow.Add(_dureeCache);
            _cache[_chiffres] = _nouvelle;
        }

        return _resultat;
    }

    private async Task<DTO_InfoEntreprise> Interroger(string p_chiffres, bool p_estSiret)
    {
        var _url = $"{URL_BASE}?q={p_chiffres}&per_page=1";
        using var _reponse = await _http.GetAsync(_url);

        if (!_reponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Annuaire des entreprises : code {Code} pour {Identifiant}", (int)_reponse.StatusCode, p_chiffres);
            return Echec("Annuaire indisponible pour le moment. Saisissez le nom à la main.");
        }

        var _corps = await _reponse.Content.ReadAsStringAsync();
        using var _doc = JsonDocument.Parse(_corps);

        JsonElement _resultats;
        if (!_doc.RootElement.TryGetProperty("results", out _resultats) || _resultats.GetArrayLength() == 0)
            return Echec("Aucune entreprise trouvée pour ce numéro. Saisissez le nom à la main.");

        var _premier = _resultats[0];

        var _info = new DTO_InfoEntreprise();
        _info.Trouve = true;
        _info.Nom = LireTexte(_premier, "nom_complet");
        if (string.IsNullOrWhiteSpace(_info.Nom))
        {
            _info.Nom = LireTexte(_premier, "nom_raison_sociale");
        }
        _info.Siren = LireTexte(_premier, "siren");

        // L'établissement saisi prime sur le siège : une agence de Bordeaux ne doit pas
        // se voir attribuer l'adresse du siège social situé à l'autre bout du pays.
        JsonElement _etablissement = default;
        bool _etablissementTrouve = false;

        if (p_estSiret)
        {
            JsonElement _correspondances;
            if (_premier.TryGetProperty("matching_etablissements", out _correspondances)
                && _correspondances.ValueKind == JsonValueKind.Array
                && _correspondances.GetArrayLength() > 0)
            {
                _etablissement = _correspondances[0];
                _etablissementTrouve = true;
            }
        }

        if (!_etablissementTrouve)
        {
            JsonElement _siege;
            if (_premier.TryGetProperty("siege", out _siege) && _siege.ValueKind == JsonValueKind.Object)
            {
                _etablissement = _siege;
                _etablissementTrouve = true;
            }
        }

        if (_etablissementTrouve)
        {
            _info.Adresse = LireTexte(_etablissement, "adresse");
            _info.Ville = LireTexte(_etablissement, "libelle_commune");
            _info.Siret = LireTexte(_etablissement, "siret");

            // "A" = actif, "F" = fermé
            var _etat = LireTexte(_etablissement, "etat_administratif");
            if (_etat == "F")
            {
                _info.EstActive = false;
            }
        }

        // Filet : on renvoie toujours le SIRET réellement saisi
        if (p_estSiret)
        {
            _info.Siret = p_chiffres;
        }

        if (string.IsNullOrWhiteSpace(_info.Nom))
            return Echec("Cette entreprise n'expose pas de nom exploitable. Saisissez-le à la main.");

        _info.Message = "Entreprise trouvée.";
        return _info;
    }

    private static string LireTexte(JsonElement p_element, string p_propriete)
    {
        JsonElement _valeur;
        if (!p_element.TryGetProperty(p_propriete, out _valeur))
            return string.Empty;
        if (_valeur.ValueKind != JsonValueKind.String)
            return string.Empty;
        var _texte = _valeur.GetString();
        if (_texte == null)
            return string.Empty;
        return _texte;
    }

    private static DTO_InfoEntreprise Echec(string p_message)
    {
        var _info = new DTO_InfoEntreprise();
        _info.Trouve = false;
        _info.Message = p_message;
        return _info;
    }

    private class Entree
    {
        public DTO_InfoEntreprise Resultat { get; set; } = new();
        public DateTime Expiration { get; set; }
    }
}
