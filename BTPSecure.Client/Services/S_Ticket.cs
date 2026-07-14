using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_Ticket
{
    private readonly HttpClient _http;

    public S_Ticket(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<List<DTO_ContactAnnuaire>> ObtenirAnnuaire()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_ContactAnnuaire>>("api/tickets/annuaire");
            if (_result == null)
                return new List<DTO_ContactAnnuaire>();
            return _result;
        }
        catch
        {
            return new List<DTO_ContactAnnuaire>();
        }
    }

    public async Task<(bool Succes, string Message)> Envoyer(DTO_EnvoyerTicket p_dto)
    {
        var _reponse = await _http.PostAsJsonAsync("api/tickets/envoyer", p_dto);
        if (_reponse.IsSuccessStatusCode)
            return (true, "Message envoyé.");
        return (false, await LireErreur(_reponse));
    }

    public async Task<List<DTO_Ticket>> ObtenirRecus()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_Ticket>>("api/tickets/recus");
            if (_result == null)
                return new List<DTO_Ticket>();
            return _result;
        }
        catch
        {
            return new List<DTO_Ticket>();
        }
    }

    public async Task<List<DTO_Ticket>> ObtenirEnvoyes()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_Ticket>>("api/tickets/envoyes");
            if (_result == null)
                return new List<DTO_Ticket>();
            return _result;
        }
        catch
        {
            return new List<DTO_Ticket>();
        }
    }

    public async Task<List<DTO_Conversation>> ObtenirConversations()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_Conversation>>("api/tickets/conversations");
            if (_result == null)
                return new List<DTO_Conversation>();
            return _result;
        }
        catch
        {
            return new List<DTO_Conversation>();
        }
    }

    public async Task<List<DTO_Ticket>> ObtenirConversation(int p_autreId)
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_Ticket>>($"api/tickets/conversation/{p_autreId}");
            if (_result == null)
                return new List<DTO_Ticket>();
            return _result;
        }
        catch
        {
            return new List<DTO_Ticket>();
        }
    }

    public async Task<int> CompterNonLus()
    {
        try
        {
            var _reponse = await _http.GetAsync("api/tickets/non-lus");
            if (!_reponse.IsSuccessStatusCode)
                return 0;
            var _obj = await _reponse.Content.ReadFromJsonAsync<CompteurReponse>();
            if (_obj == null)
                return 0;
            return _obj.Count;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<(bool Succes, string Message)> MarquerLu(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/tickets/marquer-lu/{p_id}", new { });
        if (_reponse.IsSuccessStatusCode)
            return (true, "Marqué comme lu.");
        return (false, await LireErreur(_reponse));
    }

    public async Task<DTO_PieceJointe?> ObtenirPieceJointe(int p_id)
    {
        try
        {
            var _reponse = await _http.GetAsync($"api/tickets/piece-jointe/{p_id}");
            if (!_reponse.IsSuccessStatusCode)
                return null;
            return await _reponse.Content.ReadFromJsonAsync<DTO_PieceJointe>();
        }
        catch
        {
            return null;
        }
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

    private class CompteurReponse
    {
        public int Count { get; set; }
    }

    private class MessageReponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
