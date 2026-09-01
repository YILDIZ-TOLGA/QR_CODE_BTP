using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

// Notifications personnelles affichées une seule fois, à la connexion suivante.
public class S_Notification
{
    private readonly HttpClient _http;

    public S_Notification(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<List<DTO_Notification>> ObtenirNonLues()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_Notification>>("api/notifications/non-lues");
            if (_result == null)
                return new List<DTO_Notification>();
            return _result;
        }
        catch
        {
            return new List<DTO_Notification>();
        }
    }

    public async Task MarquerLues()
    {
        try
        {
            await _http.PostAsJsonAsync("api/notifications/marquer-lues", new { });
        }
        catch { }
    }
}
