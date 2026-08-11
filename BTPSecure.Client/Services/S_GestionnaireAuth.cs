using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BTPSecure.Client.Services;

// Déconnecte l'utilisateur dès que le serveur rejette son jeton (compte bloqué par
// l'admin, jeton expiré). Sans cela il resterait sur des pages vides sans comprendre.
// Une connexion refusée renvoie 400 : un 401 signifie donc bien « jeton rejeté ».
public class S_GestionnaireAuth : DelegatingHandler
{
    private readonly IJSRuntime _js;
    private readonly NavigationManager _navigation;

    public S_GestionnaireAuth(IJSRuntime p_js, NavigationManager p_navigation)
    {
        _js = p_js;
        _navigation = p_navigation;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage p_requete, CancellationToken p_annulation)
    {
        var _reponse = await base.SendAsync(p_requete, p_annulation);

        if (_reponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "token");
            // Rechargement complet : l'état d'authentification repart de zéro
            _navigation.NavigateTo("/connexion", true);
        }

        return _reponse;
    }
}
