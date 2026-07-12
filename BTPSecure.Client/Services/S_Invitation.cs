using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_Invitation
{
    private readonly HttpClient _http;

    public S_Invitation(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<List<DTO_InvitationAffichage>> ObtenirInvitations()
    {
        var _result = await _http.GetFromJsonAsync<List<DTO_InvitationAffichage>>("api/collaborateur/invitations");
        return _result ?? new List<DTO_InvitationAffichage>();
    }

    public async Task<(bool Succes, string Message)> Accepter(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/collaborateur/accepter/{p_id}", new { });
        return await LireResultat(_reponse);
    }

    public async Task<(bool Succes, string Message)> Refuser(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/collaborateur/refuser/{p_id}", new { });
        return await LireResultat(_reponse);
    }

    public async Task<(bool Succes, string Message)> Quitter(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/collaborateur/quitter/{p_id}", new { });
        return await LireResultat(_reponse);
    }

    private static async Task<(bool, string)> LireResultat(HttpResponseMessage p_reponse)
    {
        try
        {
            var _body = await p_reponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(_body))
            {
                var _obj = System.Text.Json.JsonSerializer.Deserialize<MessageReponse>(_body,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (p_reponse.IsSuccessStatusCode, _obj?.Message ?? "Erreur.");
            }
        }
        catch { }
        return (p_reponse.IsSuccessStatusCode, p_reponse.IsSuccessStatusCode ? "OK" : "Erreur.");
    }

    private class MessageReponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
