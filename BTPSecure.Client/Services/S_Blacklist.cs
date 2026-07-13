using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_Blacklist
{
    private readonly HttpClient _http;

    public S_Blacklist(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<List<DTO_Blacklist>> Lister()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_Blacklist>>("api/blacklist");
            if (_result == null)
                return new List<DTO_Blacklist>();
            return _result;
        }
        catch
        {
            return new List<DTO_Blacklist>();
        }
    }

    public async Task<(bool Succes, string Message)> Ajouter(string p_email)
    {
        var _reponse = await _http.PostAsJsonAsync("api/blacklist/ajouter", new DTO_AjouterBlacklist { Email = p_email });
        if (_reponse.IsSuccessStatusCode)
            return (true, "Email ajouté à la blacklist.");
        return (false, await LireErreur(_reponse));
    }

    public async Task<(bool Succes, string Message)> Supprimer(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/blacklist/supprimer/{p_id}", new { });
        if (_reponse.IsSuccessStatusCode)
            return (true, "Email retiré.");
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
