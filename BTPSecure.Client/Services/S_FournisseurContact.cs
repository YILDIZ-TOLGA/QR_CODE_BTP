using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_FournisseurContact
{
    private readonly HttpClient _http;

    public S_FournisseurContact(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<List<DTO_FournisseurContact>> Lister()
    {
        try
        {
            var _liste = await _http.GetFromJsonAsync<List<DTO_FournisseurContact>>("api/fournisseurs-contacts");
            if (_liste == null)
                return new List<DTO_FournisseurContact>();
            return _liste;
        }
        catch
        {
            return new List<DTO_FournisseurContact>();
        }
    }

    public async Task<(bool Succes, string Message)> Creer(DTO_CreerFournisseurContact p_dto)
    {
        var _reponse = await _http.PostAsJsonAsync("api/fournisseurs-contacts/creer", p_dto);
        if (_reponse.IsSuccessStatusCode)
            return (true, "Fournisseur ajouté.");

        var _msg = await LireMessageErreur(_reponse);
        return (false, _msg ?? "Erreur lors de l'ajout.");
    }

    public async Task<(bool Succes, string Message)> Modifier(int p_id, DTO_CreerFournisseurContact p_dto)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/fournisseurs-contacts/modifier/{p_id}", p_dto);
        if (_reponse.IsSuccessStatusCode)
            return (true, "Fournisseur mis à jour.");

        var _msg = await LireMessageErreur(_reponse);
        return (false, _msg ?? "Erreur lors de la modification.");
    }

    public async Task<(bool Succes, string Message)> Supprimer(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/fournisseurs-contacts/supprimer/{p_id}", new { });
        if (_reponse.IsSuccessStatusCode)
            return (true, "Fournisseur supprimé.");

        var _msg = await LireMessageErreur(_reponse);
        return (false, _msg ?? "Erreur lors de la suppression.");
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

    private class MessageReponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
