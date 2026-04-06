using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BTPSecure.Client.Services;

public class S_AuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private readonly HttpClient _http;

    public S_AuthStateProvider(IJSRuntime p_js, HttpClient p_http)
    {
        _js = p_js;
        _http = p_http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var _token = await _js.InvokeAsync<string>("localStorage.getItem", "token");

        if (string.IsNullOrWhiteSpace(_token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var _claims = ParseToken(_token);
        if (_claims == null)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

        var _identity = new ClaimsIdentity(_claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(_identity));
    }

    public async Task Connecter(string p_token)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", "token", p_token);
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", p_token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task Deconnecter()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "token");
        _http.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static IEnumerable<Claim>? ParseToken(string p_token)
    {
        try
        {
            var _handler = new JwtSecurityTokenHandler();
            var _jwt = _handler.ReadJwtToken(p_token);

            if (_jwt.ValidTo < DateTime.UtcNow)
                return null;

            return _jwt.Claims;
        }
        catch
        {
            return null;
        }
    }
}
