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

        var _message = await LireMessageErreur(_reponse);
        return (false, _message ?? "Erreur lors de l'inscription.");
    }

    public async Task<(bool Succes, string Message, DTO_ReponseAuth? Reponse)> Connecter(DTO_Connexion p_dto)
    {
        var _reponse = await _http.PostAsJsonAsync("api/auth/connexion", p_dto);
        if (!_reponse.IsSuccessStatusCode)
        {
            var _message = await LireMessageErreur(_reponse);
            return (false, _message ?? "Erreur de connexion.", null);
        }

        var _auth = await _reponse.Content.ReadFromJsonAsync<DTO_ReponseAuth>();
        await _authProvider.Connecter(_auth!.Token);
        return (true, "Connexion réussie.", _auth);
    }

    private static async Task<string?> LireMessageErreur(HttpResponseMessage p_reponse)
    {
        try
        {
            var _body = await p_reponse.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(_body)) return null;
            var _erreur = System.Text.Json.JsonSerializer.Deserialize<MessageReponse>(_body,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return _erreur?.Message;
        }
        catch { return null; }
    }

    public async Task Deconnecter()
    {
        await _authProvider.Deconnecter();
    }

    public async Task<(bool Succes, string Message)> DemanderReset(string p_email)
    {
        var _reponse = await _http.PostAsJsonAsync("api/auth/demander-reset", new DTO_DemandeReset { Email = p_email });
        if (_reponse.IsSuccessStatusCode)
        {
            var _msg = await LireMessageErreur(_reponse);
            return (true, _msg ?? "Si un compte existe avec cet email, un lien vous a été envoyé.");
        }
        var _erreur = await LireMessageErreur(_reponse);
        return (false, _erreur ?? "Erreur lors de la demande.");
    }

    public async Task<(bool Succes, string Message)> ReinitialiserMotDePasse(string p_token, string p_nouveauMotDePasse)
    {
        var _reponse = await _http.PostAsJsonAsync("api/auth/reinitialiser",
            new DTO_ReinitialiserMotDePasse { Token = p_token, NouveauMotDePasse = p_nouveauMotDePasse });
        if (_reponse.IsSuccessStatusCode)
            return (true, "Mot de passe modifié avec succès.");
        var _erreur = await LireMessageErreur(_reponse);
        return (false, _erreur ?? "Erreur lors de la réinitialisation.");
    }

    private class MessageReponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
