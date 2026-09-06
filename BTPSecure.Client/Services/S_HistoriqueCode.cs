using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_HistoriqueCode
{
    private readonly HttpClient _http;

    public S_HistoriqueCode(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<List<DTO_PorteurCodePermanent>> ObtenirPorteurs()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_PorteurCodePermanent>>("api/historique-codes/porteurs");
            if (_result == null)
                return new List<DTO_PorteurCodePermanent>();
            return _result;
        }
        catch
        {
            return new List<DTO_PorteurCodePermanent>();
        }
    }

    public async Task<List<DTO_UtilisationCode>> ObtenirUtilisations(int p_porteurId)
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_UtilisationCode>>($"api/historique-codes/utilisations/{p_porteurId}");
            if (_result == null)
                return new List<DTO_UtilisationCode>();
            return _result;
        }
        catch
        {
            return new List<DTO_UtilisationCode>();
        }
    }
}
