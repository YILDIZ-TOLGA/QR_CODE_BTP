using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

// Interroge l'annuaire des entreprises via notre serveur (qui relaie et met en cache).
public class S_RechercheEntreprise
{
    private readonly HttpClient _http;

    public S_RechercheEntreprise(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<DTO_InfoEntreprise> Rechercher(string p_identifiant)
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<DTO_InfoEntreprise>($"api/recherche-entreprise/{p_identifiant}");
            if (_result == null)
            {
                var _echec = new DTO_InfoEntreprise();
                _echec.Message = "Réponse inattendue de l'annuaire.";
                return _echec;
            }
            return _result;
        }
        catch
        {
            var _echec = new DTO_InfoEntreprise();
            _echec.Message = "Annuaire injoignable. Saisissez le nom à la main.";
            return _echec;
        }
    }
}
