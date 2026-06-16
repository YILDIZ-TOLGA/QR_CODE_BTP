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

    public async Task<(bool Succes, string Message)> ChangerMotDePasse(string p_ancien, string p_nouveau)
    {
        var _reponse = await _http.PostAsJsonAsync("api/profil/changer-mot-de-passe",
            new DTO_ChangerMotDePasse { AncienMotDePasse = p_ancien, NouveauMotDePasse = p_nouveau });
        if (_reponse.IsSuccessStatusCode)
            return (true, "Mot de passe modifié avec succès.");
        try
        {
            var _body = await _reponse.Content.ReadAsStringAsync();
            var _obj = System.Text.Json.JsonSerializer.Deserialize<MessageReponse>(_body,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return (false, _obj?.Message ?? "Erreur lors du changement.");
        }
        catch
        {
            return (false, "Erreur lors du changement.");
        }
    }

    private class MessageReponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
