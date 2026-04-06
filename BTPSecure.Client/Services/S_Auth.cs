using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_Auth
{
    private readonly HttpClient _http;
    private readonly S_AuthStateProvider _authProvider;

    public S_Auth(HttpClient p_http, S_AuthStateProvider p_authProvider)
    {
        _http = p_http;
        _authProvider = p_authProvider;
    }

    public async Task<(bool Succes, string Message)> Inscrire(DTO_Inscription p_dto)
    {
        var _reponse = await _http.PostAsJsonAsync("api/auth/inscription", p_dto);
        if (_reponse.IsSuccessStatusCode)
            return (true, "Inscription réussie !");

        var _erreur = await _reponse.Content.ReadFromJsonAsync<MessageReponse>();
        return (false, _erreur?.Message ?? "Erreur lors de l'inscription.");
    }

    public async Task<(bool Succes, string Message, DTO_ReponseAuth? Reponse)> Connecter(DTO_Connexion p_dto)
    {
        var _reponse = await _http.PostAsJsonAsync("api/auth/connexion", p_dto);
        if (!_reponse.IsSuccessStatusCode)
        {
            var _erreur = await _reponse.Content.ReadFromJsonAsync<MessageReponse>();
            return (false, _erreur?.Message ?? "Erreur de connexion.", null);
        }

        var _auth = await _reponse.Content.ReadFromJsonAsync<DTO_ReponseAuth>();
        await _authProvider.Connecter(_auth!.Token);
        return (true, "Connexion réussie.", _auth);
    }

    public async Task Deconnecter()
    {
        await _authProvider.Deconnecter();
    }

    private class MessageReponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
