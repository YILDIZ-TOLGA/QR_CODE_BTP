using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_Memo
{
    private readonly HttpClient _http;

    public S_Memo(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<List<DTO_Memo>> Lister()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_Memo>>("api/memos");
            if (_result == null)
                return new List<DTO_Memo>();
            return _result;
        }
        catch
        {
            return new List<DTO_Memo>();
        }
    }

    public async Task<(bool Succes, string Message)> Enregistrer(DTO_EnregistrerMemo p_dto)
    {
        var _reponse = await _http.PostAsJsonAsync("api/memos/enregistrer", p_dto);
        if (_reponse.IsSuccessStatusCode)
            return (true, "Note enregistrée.");
        return (false, await LireErreur(_reponse));
    }

    public async Task<(bool Succes, string Message)> Supprimer(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/memos/supprimer/{p_id}", new { });
        if (_reponse.IsSuccessStatusCode)
            return (true, "Note supprimée.");
        return (false, await LireErreur(_reponse));
    }

    private static async Task<string> LireErreur(HttpResponseMessage p_reponse)
    {
        try
        {
            var _body = await p_reponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(_body))
            {
                var _erreur = System.Text.Json.JsonSerializer.Deserialize<MessageReponse>(_body,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (_erreur != null && !string.IsNullOrEmpty(_erreur.Message))
                    return _erreur.Message;
            }
        }
        catch { }
        return "Une erreur est survenue.";
    }

    private class MessageReponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
