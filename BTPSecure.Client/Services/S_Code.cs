using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_Code
{
    private readonly HttpClient _http;

    public S_Code(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<(bool Succes, string Message, DTO_CodeAffichage? Code)> Creer(DTO_CreerCode p_dto)
    {
        var _reponse = await _http.PostAsJsonAsync("api/codes/creer", p_dto);
        if (!_reponse.IsSuccessStatusCode)
        {
            var _erreur = await LireErreur(_reponse);
            return (false, _erreur, null);
        }
        var _code = await _reponse.Content.ReadFromJsonAsync<DTO_CodeAffichage>();
        return (true, "Code créé avec succès.", _code);
    }

    public async Task<List<DTO_CodeAffichage>> ObtenirParPatron()
    {
        var _result = await _http.GetFromJsonAsync<List<DTO_CodeAffichage>>("api/codes/patron");
        return _result ?? new List<DTO_CodeAffichage>();
    }

    public async Task<List<DTO_CodeAffichage>> ObtenirParSalarie()
    {
        var _result = await _http.GetFromJsonAsync<List<DTO_CodeAffichage>>("api/codes/salarie");
        return _result ?? new List<DTO_CodeAffichage>();
    }

    public async Task<(bool Succes, DTO_ResultatValidation? Resultat)> Valider(string p_valeur)
    {
        var _reponse = await _http.PostAsJsonAsync("api/codes/valider", new DTO_ValiderCode { Valeur = p_valeur });
        var _resultat = await _reponse.Content.ReadFromJsonAsync<DTO_ResultatValidation>();
        return (_reponse.IsSuccessStatusCode, _resultat);
    }

    public async Task<(bool Succes, string Message)> Revoquer(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/codes/revoquer/{p_id}", new { });
        if (!_reponse.IsSuccessStatusCode)
        {
            var _erreur = await LireErreur(_reponse);
            return (false, _erreur);
        }
        return (true, "Code révoqué.");
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
