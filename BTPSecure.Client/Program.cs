using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using BTPSecure.Client;
using BTPSecure.Client.Services;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Le gestionnaire intercepte les 401 : un compte bloqué est déconnecté immédiatement.
// Il ne dépend que de IJSRuntime/NavigationManager pour éviter une boucle avec HttpClient.
builder.Services.AddScoped<S_GestionnaireAuth>();
builder.Services.AddScoped(sp =>
{
    var _gestionnaire = sp.GetRequiredService<S_GestionnaireAuth>();
    _gestionnaire.InnerHandler = new HttpClientHandler();
    return new HttpClient(_gestionnaire) { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
});

builder.Services.AddScoped<S_AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<S_AuthStateProvider>());
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<S_Auth>();
builder.Services.AddScoped<S_Code>();
builder.Services.AddScoped<S_Entreprise>();
builder.Services.AddScoped<S_Admin>();
builder.Services.AddScoped<S_Invitation>();
builder.Services.AddScoped<S_Profil>();
builder.Services.AddScoped<S_FournisseurContact>();
builder.Services.AddScoped<S_SousCompte>();
builder.Services.AddScoped<S_Blacklist>();
builder.Services.AddScoped<S_Ticket>();
builder.Services.AddScoped<S_EtatUi>();
builder.Services.AddScoped<S_Memo>();
builder.Services.AddScoped<S_Acces>();
builder.Services.AddScoped<S_Notification>();
builder.Services.AddScoped<S_RechercheEntreprise>();

builder.Services.AddRadzenComponents();

await builder.Build().RunAsync();
