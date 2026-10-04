using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_Admin
{
    private readonly HttpClient _http;

    public S_Admin(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<List<DTO_EntrepriseAdmin>> ObtenirEntreprises()
    {
        var _result = await _http.GetFromJsonAsync<List<DTO_EntrepriseAdmin>>("api/admin/entreprises");
        return _result ?? new List<DTO_EntrepriseAdmin>();
    }

    // p_montantCommission n'est pris en compte par le serveur que si l'entreprise est
    // parrainee et que son parrainage est encore en attente.
    public async Task<(bool Succes, string Message)> BasculerAutorisation(int p_entrepriseId, decimal p_montantCommission)
    {
        var _dto = new DTO_AutoriserEntreprise();
        _dto.MontantCommission = p_montantCommission;
        var _reponse = await _http.PostAsJsonAsync($"api/admin/basculer-autorisation/{p_entrepriseId}", _dto);
        if (!_reponse.IsSuccessStatusCode)
        {
            try
            {
                var _body = await _reponse.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(_body))
                {
                    var _erreur = System.Text.Json.JsonSerializer.Deserialize<MessageReponse>(_body,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return (false, _erreur?.Message ?? "Erreur.");
                }
            }
            catch { }
            return (false, "Erreur lors de la modification.");
        }
        return (true, "Autorisation modifiée.");
    }

    public async Task<List<DTO_FournisseurAdmin>> ObtenirFournisseurs()
    {
        var _result = await _http.GetFromJsonAsync<List<DTO_FournisseurAdmin>>("api/admin/fournisseurs");
        return _result ?? new List<DTO_FournisseurAdmin>();
    }

    public async Task<(bool Succes, string Message)> ValiderFournisseur(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/admin/valider-fournisseur/{p_id}", new { });
        if (_reponse.IsSuccessStatusCode)
            return (true, "Fournisseur validé.");
        return (false, await LireErreur(_reponse));
    }

    // Bloque / débloque un fournisseur (et ses sous-comptes s'il est principal)
    public async Task<(bool Succes, string Message)> BasculerBlocageFournisseur(int p_id)
    {
        var _reponse = await _http.PostAsJsonAsync($"api/admin/basculer-blocage-fournisseur/{p_id}", new { });
        if (_reponse.IsSuccessStatusCode)
        {
            // Le serveur précise combien de sous-comptes ont suivi
            var _message = await LireMessage(_reponse);
            if (string.IsNullOrEmpty(_message))
                return (true, "Statut du fournisseur modifié.");
            return (true, _message);
        }
        return (false, await LireErreur(_reponse));
    }

    private static async Task<string> LireMessage(HttpResponseMessage p_reponse)
    {
        try
        {
            var _body = await p_reponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(_body))
            {
                var _obj = System.Text.Json.JsonSerializer.Deserialize<MessageReponse>(_body,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (_obj != null && !string.IsNullOrEmpty(_obj.Message))
                    return _obj.Message;
            }
        }
        catch { }
        return string.Empty;
    }

    public async Task<(bool Succes, string Message)> ChangerLimiteResponsables(int p_id, int p_limite)
    {
        var _dto = new DTO_LimiteResponsables();
        _dto.Limite = p_limite;
        var _reponse = await _http.PostAsJsonAsync($"api/admin/limite-responsables/{p_id}", _dto);
        if (_reponse.IsSuccessStatusCode)
            return (true, "Limite modifiée.");
        return (false, await LireErreur(_reponse));
    }

    public async Task<(bool Succes, string Message)> ChangerLimiteSousComptes(int p_id, int p_limite)
    {
        var _dto = new DTO_LimiteSousComptes();
        _dto.Limite = p_limite;
        var _reponse = await _http.PostAsJsonAsync($"api/admin/limite-souscomptes/{p_id}", _dto);
        if (_reponse.IsSuccessStatusCode)
            return (true, "Limite modifiée.");
        return (false, await LireErreur(_reponse));
    }

    // ---------- Administration des comptes ----------

    public async Task<List<DTO_CompteAdmin>> ObtenirComptes()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_CompteAdmin>>("api/admin/comptes");
            if (_result == null)
                return new List<DTO_CompteAdmin>();
            return _result;
        }
        catch
        {
            return new List<DTO_CompteAdmin>();
        }
    }

    public async Task<DTO_ApercuSuppression?> ObtenirApercuSuppression(int p_id)
    {
        try
        {
            return await _http.GetFromJsonAsync<DTO_ApercuSuppression>($"api/admin/apercu-suppression/{p_id}");
        }
        catch
        {
            return null;
        }
    }

    // p_email : l'email du compte, retape par l'admin. Reverifie cote serveur.
    public async Task<(bool Succes, string Message)> SupprimerCompte(int p_id, string p_email)
    {
        var _dto = new DTO_ConfirmerSuppression();
        _dto.Email = p_email;
        var _reponse = await _http.PostAsJsonAsync($"api/admin/supprimer-compte/{p_id}", _dto);
        if (!_reponse.IsSuccessStatusCode)
            return (false, await LireErreur(_reponse));

        var _ok = await _reponse.Content.ReadFromJsonAsync<MessageReponse>();
        if (_ok == null)
            return (true, "Compte supprime.");
        return (true, _ok.Message);
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
                return _erreur?.Message ?? "Erreur.";
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
