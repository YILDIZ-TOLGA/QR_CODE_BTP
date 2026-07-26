using System.Net.Http.Json;
using BTPSecure.Shared.DTOs;

namespace BTPSecure.Client.Services;

// Portail d'accès de la phase de test (beta). Voir Comp_AccesBeta + le garde dans MainLayout.
public class S_Acces
{
    private readonly HttpClient _http;

    public S_Acces(HttpClient p_http)
    {
        _http = p_http;
    }

    // Le portail est-il actif côté serveur ?
    // En cas d'erreur réseau on renvoie false : on ne bloque jamais un utilisateur à cause d'un endpoint capricieux.
    public async Task<bool> EstActif()
    {
        try
        {
            var _statut = await _http.GetFromJsonAsync<StatutAcces>("api/acces/statut");
            if (_statut == null)
                return false;
            return _statut.Actif;
        }
        catch
        {
            return false;
        }
    }

    // Vérifie le code saisi auprès du serveur (le vrai code n'est jamais côté client).
    public async Task<bool> Verifier(string p_code)
    {
        try
        {
            var _dto = new DTO_CodeAcces();
            _dto.Code = p_code;
            var _reponse = await _http.PostAsJsonAsync("api/acces/verifier", _dto);
            return _reponse.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private class StatutAcces
    {
        public bool Actif { get; set; }
    }
}
