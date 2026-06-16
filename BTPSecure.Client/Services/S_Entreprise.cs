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

    public async Task<(bool Succes, string Message)> CreerSalarie(DTO_CreerSalarie p_dto)
    {
        var _reponse = await _http.PostAsJsonAsync("api/entreprises/creer-salarie", p_dto);
        if (_reponse.IsSuccessStatusCode)
            return (true, "Salarié créé. Il recevra ses identifiants par email.");
        var _erreur = await LireErreur(_reponse);
        return (false, _erreur);
    }

    public async Task<(bool Succes, string Message, DTO_SalarieAffichage? Salarie)> AjouterSalarie(string p_email)
    {
        var _reponse = await _http.PostAsJsonAsync("api/entreprises/ajouter-salarie", new DTO_AjouterSalarie { Email = p_email });
        if (!_reponse.IsSuccessStatusCode)
        {
            var _erreur = await LireErreur(_reponse);
            return (false, _erreur, null);
        }
        var _salarie = await _reponse.Content.ReadFromJsonAsync<DTO_SalarieAffichage>();
        return (true, "Salarié ajouté.", _salarie);
    }

    public async Task<List<DTO_SalarieAffichage>> ObtenirSalaries()
    {
        var _result = await _http.GetFromJsonAsync<List<DTO_SalarieAffichage>>("api/entreprises/salaries");
        return _result ?? new List<DTO_SalarieAffichage>();
    }

    public async Task<(bool Succes, string Message)> RetirerSalarie(int p_id)
    {
        var _reponse = await _http.DeleteAsync($"api/entreprises/retirer-salarie/{p_id}");
        if (!_reponse.IsSuccessStatusCode)
        {
            var _erreur = await LireErreur(_reponse);
            return (false, _erreur);
        }
        return (true, "Salarié retiré.");
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
