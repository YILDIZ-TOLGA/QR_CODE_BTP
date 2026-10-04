using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_NotificationGlobale
{
    private readonly HttpClient _http;

    public S_NotificationGlobale(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<List<DTO_NotificationGlobaleAdmin>> ObtenirToutes()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_NotificationGlobaleAdmin>>("api/notifications-globales");
            if (_result == null)
                return new List<DTO_NotificationGlobaleAdmin>();
            return _result;
        }
        catch
        {
            return new List<DTO_NotificationGlobaleAdmin>();
        }
    }

    public async Task<List<string>> ObtenirRoles()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<string>>("api/notifications-globales/roles");
            if (_result == null)
                return new List<string>();
            return _result;
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<(bool Succes, string Message)> Creer(DTO_CreerNotificationGlobale p_dto)
    {
        var _reponse = await _http.PostAsJsonAsync("api/notifications-globales/creer", p_dto);
        if (!_reponse.IsSuccessStatusCode)
            return (false, await LireErreur(_reponse));
        return (true, "Notification programmée.");
    }

    public async Task<(bool Succes, string Message)> BasculerActivation(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/notifications-globales/basculer/{p_id}", new { });
        if (!_reponse.IsSuccessStatusCode)
            return (false, await LireErreur(_reponse));

        var _ok = await _reponse.Content.ReadFromJsonAsync<MessageReponse>();
        if (_ok == null)
            return (true, "Modifié.");
        return (true, _ok.Message);
    }

    public async Task<(bool Succes, string Message)> Supprimer(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/notifications-globales/supprimer/{p_id}", new { });
        if (!_reponse.IsSuccessStatusCode)
            return (false, await LireErreur(_reponse));
        return (true, "Notification supprimée.");
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
                if (_erreur != null)
                    return _erreur.Message;
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
