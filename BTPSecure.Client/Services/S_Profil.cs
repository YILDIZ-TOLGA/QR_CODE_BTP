using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_Profil
{
    private readonly HttpClient _http;

    public S_Profil(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<DTO_Profil?> ObtenirMoi()
    {
        try
        {
            var _reponse = await _http.GetAsync("api/profil/moi");
            if (!_reponse.IsSuccessStatusCode)
                return null;
            return await _reponse.Content.ReadFromJsonAsync<DTO_Profil>();
        }
        catch
        {
            return null;
        }
    }
}
