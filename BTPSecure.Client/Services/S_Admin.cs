using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_Admin
{
    private readonly HttpClient _http;

    public S_Admin(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<List<DTO_EntrepriseAdmin>> ObtenirEntreprises()
    {
        var _result = await _http.GetFromJsonAsync<List<DTO_EntrepriseAdmin>>("api/admin/entreprises");
        return _result ?? new List<DTO_EntrepriseAdmin>();
    }

    public async Task<(bool Succes, string Message)> BasculerAutorisation(int p_entrepriseId)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/admin/basculer-autorisation/{p_entrepriseId}", new { });
        if (!_reponse.IsSuccessStatusCode)
        {
            try
            {
                var _body = await _reponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(_body))
                {
                    var _erreur = System.Text.Json.JsonSerializer.Deserialize<MessageReponse>(_body,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (false, _erreur?.Message ?? "Erreur.");
                }
            }
            catch { }
            return (false, "Erreur lors de la modification.");
        }
        return (true, "Autorisation modifiée.");
    }

    public async Task<List<DTO_FournisseurAdmin>> ObtenirFournisseurs()
    {
        var _result = await _http.GetFromJsonAsync<List<DTO_FournisseurAdmin>>("api/admin/fournisseurs");
        return _result ?? new List<DTO_FournisseurAdmin>();
    }

    public async Task<(bool Succes, string Message)> ValiderFournisseur(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/admin/valider-fournisseur/{p_id}", new { });
        if (_reponse.IsSuccessStatusCode)
            return (true, "Fournisseur validé.");
        return (false, await LireErreur(_reponse));
    }

    public async Task<(bool Succes, string Message)> DesactiverFournisseur(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/admin/desactiver-fournisseur/{p_id}", new { });
        if (_reponse.IsSuccessStatusCode)
            return (true, "Fournisseur désactivé.");
        return (false, await LireErreur(_reponse));
    }

    public async Task<(bool Succes, string Message)> ChangerLimiteSousComptes(int p_id, int p_limite)
    {
        var _dto = new DTO_LimiteSousComptes();
        _dto.Limite = p_limite;
        var _reponse = await _http.PostAsJsonAsync($"api/admin/limite-souscomptes/{p_id}", _dto);
        if (_reponse.IsSuccessStatusCode)
            return (true, "Limite modifiée.");
        return (false, await LireErreur(_reponse));
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

    private class MessageReponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
