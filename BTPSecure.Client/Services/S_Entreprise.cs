using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_Entreprise
{
    private readonly HttpClient _http;

    public S_Entreprise(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<(bool Succes, string Message, DTO_EntrepriseAffichage? Entreprise)> Creer(DTO_CreerEntreprise p_dto)
    {
        var _reponse = await _http.PostAsJsonAsync("api/entreprises/creer", p_dto);
        if (!_reponse.IsSuccessStatusCode)
        {
            var _erreur = await LireErreur(_reponse);
            return (false, _erreur, null);
        }
        var _entreprise = await _reponse.Content.ReadFromJsonAsync<DTO_EntrepriseAffichage>();
        return (true, "Entreprise créée.", _entreprise);
    }

    public async Task<DTO_EntrepriseAffichage?> ObtenirMonEntreprise()
    {
        try
        {
            var _reponse = await _http.GetAsync("api/entreprises/ma-entreprise");
            if (!_reponse.IsSuccessStatusCode)
                return null;
            var _body = await _reponse.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(_body) || _body == "null")
                return null;
            return System.Text.Json.JsonSerializer.Deserialize<DTO_EntrepriseAffichage>(_body,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }

    public async Task<(bool Succes, string Message)> CreerCollaborateur(DTO_CreerCollaborateur p_dto)
    {
        var _reponse = await _http.PostAsJsonAsync("api/entreprises/creer-collaborateur", p_dto);
        if (_reponse.IsSuccessStatusCode)
            return (true, "Collaborateur créé. Il recevra ses identifiants par email.");
        var _erreur = await LireErreur(_reponse);
        return (false, _erreur);
    }

    public async Task<(bool Succes, string Message, DTO_CollaborateurAffichage? Collaborateur)> AjouterCollaborateur(string p_email)
    {
        var _reponse = await _http.PostAsJsonAsync("api/entreprises/ajouter-collaborateur", new DTO_AjouterCollaborateur { Email = p_email });
        if (!_reponse.IsSuccessStatusCode)
        {
            var _erreur = await LireErreur(_reponse);
            return (false, _erreur, null);
        }
        var _collaborateur = await _reponse.Content.ReadFromJsonAsync<DTO_CollaborateurAffichage>();
        return (true, "Collaborateur ajouté.", _collaborateur);
    }

    public async Task<List<DTO_CollaborateurAffichage>> ObtenirCollaborateurs()
    {
        var _result = await _http.GetFromJsonAsync<List<DTO_CollaborateurAffichage>>("api/entreprises/collaborateurs");
        return _result ?? new List<DTO_CollaborateurAffichage>();
    }

    public async Task<(bool Succes, string Message)> RetirerCollaborateur(int p_id)
    {
        var _reponse = await _http.DeleteAsync($"api/entreprises/retirer-collaborateur/{p_id}");
        if (!_reponse.IsSuccessStatusCode)
        {
            var _erreur = await LireErreur(_reponse);
            return (false, _erreur);
        }
        return (true, "Collaborateur retiré.");
    }

    private static async Task<string> LireErreur(HttpResponseMessage p_reponse)
    {
        try
        {
            var _obj = await p_reponse.Content.ReadFromJsonAsync<MessageReponse>();
            return _obj?.Message ?? "Une erreur est survenue.";
        }
        catch
        {
            return "Une erreur est survenue.";
        }
    }

    private class MessageReponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
