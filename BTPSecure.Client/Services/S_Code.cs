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

    public async Task<List<DTO_CodeAffichage>> ObtenirParDirigeant()
    {
        var _result = await _http.GetFromJsonAsync<List<DTO_CodeAffichage>>("api/codes/dirigeant");
        return _result ?? new List<DTO_CodeAffichage>();
    }

    public async Task<DTO_ContexteCreationCode> ObtenirContexteCreation()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<DTO_ContexteCreationCode>("api/codes/contexte-creation");
            if (_result == null)
                return new DTO_ContexteCreationCode { PeutCreer = false };
            return _result;
        }
        catch
        {
            return new DTO_ContexteCreationCode { PeutCreer = false };
        }
    }

    public async Task<DTO_ContexteDashboard> ObtenirContexteDashboard()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<DTO_ContexteDashboard>("api/codes/contexte-dashboard");
            if (_result == null)
                return new DTO_ContexteDashboard();
            return _result;
        }
        catch
        {
            return new DTO_ContexteDashboard();
        }
    }

    public async Task<List<DTO_CodeAffichage>> ObtenirParCollaborateur()
    {
        var _result = await _http.GetFromJsonAsync<List<DTO_CodeAffichage>>("api/codes/collaborateur");
        return _result ?? new List<DTO_CodeAffichage>();
    }

    public async Task<(bool Succes, DTO_ResultatValidation? Resultat)> ValiderPourCommande(int p_codeId, string p_valeur)
    {
        var _reponse = await _http.PostAsJsonAsync("api/codes/valider-pour-commande",
            new DTO_ValiderPourCommande { CodeId = p_codeId, Valeur = p_valeur });
        DTO_ResultatValidation? _resultat = null;
        try
        {
            _resultat = await _reponse.Content.ReadFromJsonAsync<DTO_ResultatValidation>();
        }
        catch { }
        return (_reponse.IsSuccessStatusCode, _resultat);
    }

    public async Task<List<DTO_CommandeAVenir>> ObtenirCommandesAVenir()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_CommandeAVenir>>("api/codes/commandes-a-venir");
            if (_result == null)
                return new List<DTO_CommandeAVenir>();
            return _result;
        }
        catch
        {
            return new List<DTO_CommandeAVenir>();
        }
    }

    public async Task<(bool Succes, string Message)> MarquerPrete(int p_codeId)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/codes/marquer-prete/{p_codeId}", new { });
        if (_reponse.IsSuccessStatusCode)
            return (true, "Commande marquée comme prête.");
        var _msg = await LireErreur(_reponse);
        return (false, _msg);
    }

    public async Task<List<DTO_NotificationDirigeant>> ObtenirNotificationsDirigeant()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_NotificationDirigeant>>("api/codes/notifications-dirigeant");
            if (_result == null)
                return new List<DTO_NotificationDirigeant>();
            return _result;
        }
        catch
        {
            return new List<DTO_NotificationDirigeant>();
        }
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

    public async Task<(bool Succes, string Message, DTO_ResultatModificationCode? Resultat)> Modifier(DTO_ModifierCode p_dto)
    {
        var _reponse = await _http.PostAsJsonAsync("api/codes/modifier", p_dto);
        if (!_reponse.IsSuccessStatusCode)
        {
            var _erreur = await LireErreur(_reponse);
            return (false, _erreur, null);
        }

        var _resultat = await _reponse.Content.ReadFromJsonAsync<DTO_ResultatModificationCode>();
        if (_resultat == null)
            return (true, "Code modifié.", null);
        return (true, _resultat.Message, _resultat);
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
