using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_SousCompte
{
    private readonly HttpClient _http;

    public S_SousCompte(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<bool> EstPrincipal()
    {
        try
        {
            var _reponse = await _http.GetAsync("api/souscompte/est-principal");
            if (!_reponse.IsSuccessStatusCode)
                return false;
            var _obj = await _reponse.Content.ReadFromJsonAsync<StatutReponse>();
            if (_obj == null)
                return false;
            return _obj.EstPrincipal;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<DTO_SousCompte>> Lister()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_SousCompte>>("api/souscompte");
            if (_result == null)
                return new List<DTO_SousCompte>();
            return _result;
        }
        catch
        {
            return new List<DTO_SousCompte>();
        }
    }

    public async Task<(bool Succes, string Message)> Creer(DTO_CreerSousCompte p_dto)
    {
        var _reponse = await _http.PostAsJsonAsync("api/souscompte/creer", p_dto);
        if (_reponse.IsSuccessStatusCode)
            return (true, "Sous-compte créé.");
        return (false, await LireErreur(_reponse));
    }

    public async Task<(bool Succes, string Message)> Desactiver(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/souscompte/desactiver/{p_id}", new { });
        if (_reponse.IsSuccessStatusCode)
            return (true, "Sous-compte désactivé.");
        return (false, await LireErreur(_reponse));
    }

    public async Task<(bool Succes, string Message)> Reactiver(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/souscompte/reactiver/{p_id}", new { });
        if (_reponse.IsSuccessStatusCode)
            return (true, "Sous-compte réactivé.");
        return (false, await LireErreur(_reponse));
    }

    public async Task<List<DTO_ValidationHistorique>> ObtenirHistorique()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_ValidationHistorique>>("api/souscompte/historique");
            if (_result == null)
                return new List<DTO_ValidationHistorique>();
            return _result;
        }
        catch
        {
            return new List<DTO_ValidationHistorique>();
        }
    }

    private static async Task<string> LireErreur(HttpResponseMessage p_reponse)
    {
        try
        {
            var _body = await p_reponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(_body))
            {
                var _erreur = System.Text.Json.JsonSerializer.Deserialize<MessageReponse>(_body,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return _erreur?.Message ?? "Erreur.";
            }
        }
        catch { }
        return "Une erreur est survenue.";
    }

    private class StatutReponse
    {
        public bool EstPrincipal { get; set; }
    }

    private class MessageReponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
