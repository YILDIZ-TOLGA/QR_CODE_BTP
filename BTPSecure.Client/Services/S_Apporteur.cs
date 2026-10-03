using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

public class S_Apporteur
{
    private readonly HttpClient _http;

    public S_Apporteur(HttpClient p_http)
    {
        _http = p_http;
    }

    public async Task<DTO_TableauApporteur> ObtenirTableauDeBord()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<DTO_TableauApporteur>("api/apporteur/tableau-de-bord");
            if (_result == null)
                return new DTO_TableauApporteur();
            return _result;
        }
        catch
        {
            return new DTO_TableauApporteur();
        }
    }

    // Vue admin : la liste des apporteurs et ce qu'ils ont cumulé
    public async Task<List<DTO_ApporteurAdmin>> ObtenirApporteursAdmin()
    {
        try
        {
            var _result = await _http.GetFromJsonAsync<List<DTO_ApporteurAdmin>>("api/admin/apporteurs");
            if (_result == null)
                return new List<DTO_ApporteurAdmin>();
            return _result;
        }
        catch
        {
            return new List<DTO_ApporteurAdmin>();
        }
    }
}
