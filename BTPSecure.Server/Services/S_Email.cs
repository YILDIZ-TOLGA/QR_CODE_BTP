using System.Net.Http.Json;
using System.Text.Json;

namespace BTPSecure.Server.Services;

public class S_Email
{
    private readonly IConfiguration _config;
    private readonly ILogger<S_Email> _logger;
    private readonly HttpClient _http;

    public S_Email(IConfiguration p_config, ILogger<S_Email> p_logger)
    {
        _config = p_config;
        _logger = p_logger;
        _http = new HttpClient { BaseAddress = new Uri("https://api.brevo.com/") };
    }

    public async Task<bool> EnvoyerVerificationEmail(string p_emailDestinataire, string p_prenom, string p_token)
    {
        var _apiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY") ?? _config["Brevo:ApiKey"];
        var _fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM") ?? _config["Brevo:FromEmail"] ?? "contact@codebtpsecure.cloud";
        var _fromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? _config["Brevo:FromName"] ?? "BTPSecure";
        var _siteUrl = Environment.GetEnvironmentVariable("SITE_URL") ?? _config["Site:Url"] ?? "https://www.codebtpsecure.cloud";

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("BREVO_API_KEY manquante, email vérification non envoyé à {Email}", p_emailDestinataire);
            return false;
        }

        var _lien = $"{_siteUrl}/verifier-email?token={p_token}";

        var _corpsHtml = $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; color: #333;"">
    <div style=""background: #1565C0; color: white; padding: 20px; border-radius: 8px 8px 0 0; text-align: center;"">
        <h1 style=""margin: 0; font-size: 24px;"">🛡️ BTPSecure</h1>
        <p style=""margin: 8px 0 0 0; opacity: 0.9;"">Vérification de votre email</p>
    </div>

    <div style=""background: #fff; padding: 24px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 8px 8px;"">
        <h2 style=""color: #1565C0; margin-top: 0;"">Bienvenue {p_prenom} !</h2>

        <p>Merci de vous être inscrit sur BTPSecure.</p>

        <p>Pour activer votre compte et vous connecter, veuillez confirmer votre adresse email en cliquant sur le bouton ci-dessous :</p>

        <p style=""text-align: center; margin: 32px 0;"">
            <a href=""{_lien}"" style=""display: inline-block; background: #1565C0; color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-weight: 600;"">
                Vérifier mon email
            </a>
        </p>

        <p style=""color: #666; font-size: 14px;"">
            Ce lien est valide pendant <strong>24 heures</strong>. Passé ce délai, vous devrez demander un nouveau lien depuis la page de connexion.
        </p>

        <p style=""color: #666; font-size: 14px;"">
            Si vous n'êtes pas à l'origine de cette inscription, ignorez simplement cet email.
        </p>

        <hr style=""border: none; border-top: 1px solid #e0e0e0; margin: 24px 0;"">

        <p style=""color: #999; font-size: 12px;"">
            Si le bouton ne fonctionne pas, copiez ce lien dans votre navigateur :<br>
            <span style=""word-break: break-all;"">{_lien}</span>
        </p>

        <p style=""color: #999; font-size: 12px; text-align: center; margin: 16px 0 0 0;"">
            Cet email est envoyé automatiquement par BTPSecure. Ne pas répondre directement.
        </p>
    </div>
</body>
</html>";

        var _payload = new
        {
            sender = new { name = _fromName, email = _fromEmail },
            to = new[] { new { email = p_emailDestinataire } },
            subject = "Vérification de votre email BTPSecure",
            htmlContent = _corpsHtml
        };

        try
        {
            var _request = new HttpRequestMessage(HttpMethod.Post, "v3/smtp/email")
            {
                Content = JsonContent.Create(_payload)
            };
            _request.Headers.Add("api-key", _apiKey);
            _request.Headers.Add("accept", "application/json");

            var _reponse = await _http.SendAsync(_request);

            if (_reponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email vérification envoyé à {Email}", p_emailDestinataire);
                return true;
            }
            else
            {
                var _body = await _reponse.Content.ReadAsStringAsync();
                _logger.LogError("Erreur Brevo vérification {Status} pour {Email} : {Body}", _reponse.StatusCode, p_emailDestinataire, _body);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception envoi email vérification à {Email}", p_emailDestinataire);
            return false;
        }
    }

    public async Task<bool> EnvoyerResetMotDePasse(string p_emailDestinataire, string p_prenom, string p_token)
    {
        var _apiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY") ?? _config["Brevo:ApiKey"];
        var _fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM") ?? _config["Brevo:FromEmail"] ?? "contact@codebtpsecure.cloud";
        var _fromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? _config["Brevo:FromName"] ?? "BTPSecure";
        var _siteUrl = Environment.GetEnvironmentVariable("SITE_URL") ?? _config["Site:Url"] ?? "https://www.codebtpsecure.cloud";

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("BREVO_API_KEY manquante, reset email non envoyé à {Email}", p_emailDestinataire);
            return false;
        }

        var _lien = $"{_siteUrl}/reinitialiser-mot-de-passe?token={p_token}";

        var _corpsHtml = $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; color: #333;"">
    <div style=""background: #1565C0; color: white; padding: 20px; border-radius: 8px 8px 0 0; text-align: center;"">
        <h1 style=""margin: 0; font-size: 24px;"">🛡️ BTPSecure</h1>
        <p style=""margin: 8px 0 0 0; opacity: 0.9;"">Réinitialisation de mot de passe</p>
    </div>

    <div style=""background: #fff; padding: 24px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 8px 8px;"">
        <h2 style=""color: #1565C0; margin-top: 0;"">Bonjour {p_prenom},</h2>

        <p>Vous avez demandé la réinitialisation du mot de passe de votre compte BTPSecure.</p>

        <p>Cliquez sur le bouton ci-dessous pour définir un nouveau mot de passe :</p>

        <p style=""text-align: center; margin: 32px 0;"">
            <a href=""{_lien}"" style=""display: inline-block; background: #1565C0; color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; font-weight: 600;"">
                Réinitialiser mon mot de passe
            </a>
        </p>

        <p style=""color: #666; font-size: 14px;"">
            Ce lien est valide pendant <strong>1 heure</strong>. Passé ce délai, vous devrez refaire une demande.
        </p>

        <p style=""color: #666; font-size: 14px;"">
            Si vous n'êtes pas à l'origine de cette demande, ignorez simplement cet email. Votre mot de passe ne sera pas modifié.
        </p>

        <hr style=""border: none; border-top: 1px solid #e0e0e0; margin: 24px 0;"">

        <p style=""color: #999; font-size: 12px;"">
            Si le bouton ne fonctionne pas, copiez ce lien dans votre navigateur :<br>
            <span style=""word-break: break-all;"">{_lien}</span>
        </p>

        <p style=""color: #999; font-size: 12px; text-align: center; margin: 16px 0 0 0;"">
            Cet email est envoyé automatiquement par BTPSecure. Ne pas répondre directement.
        </p>
    </div>
</body>
</html>";

        var _payload = new
        {
            sender = new { name = _fromName, email = _fromEmail },
            to = new[] { new { email = p_emailDestinataire } },
            subject = "Réinitialisation de votre mot de passe BTPSecure",
            htmlContent = _corpsHtml
        };

        try
        {
            var _request = new HttpRequestMessage(HttpMethod.Post, "v3/smtp/email")
            {
                Content = JsonContent.Create(_payload)
            };
            _request.Headers.Add("api-key", _apiKey);
            _request.Headers.Add("accept", "application/json");

            var _reponse = await _http.SendAsync(_request);

            if (_reponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email reset envoyé à {Email}", p_emailDestinataire);
                return true;
            }
            else
            {
                var _body = await _reponse.Content.ReadAsStringAsync();
                _logger.LogError("Erreur Brevo reset {Status} pour {Email} : {Body}", _reponse.StatusCode, p_emailDestinataire, _body);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception envoi email reset à {Email}", p_emailDestinataire);
            return false;
        }
    }

    public async Task<bool> EnvoyerInvitationFournisseur(string p_emailDestinataire, string p_nomEntreprisePatron,
        string p_nomEntrepriseFournisseur, string p_siret, string? p_siren, string p_emailFournisseurAssocie)
    {
        var _apiKey = Environment.GetEnvironmentVariable("BREVO_API_KEY") ?? _config["Brevo:ApiKey"];
        var _fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM") ?? _config["Brevo:FromEmail"] ?? "contact@codebtpsecure.cloud";
        var _fromName = Environment.GetEnvironmentVariable("SMTP_FROM_NAME") ?? _config["Brevo:FromName"] ?? "BTPSecure";
        var _siteUrl = Environment.GetEnvironmentVariable("SITE_URL") ?? _config["Site:Url"] ?? "https://www.codebtpsecure.cloud";

        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("BREVO_API_KEY manquante, email non envoyé à {Email}", p_emailDestinataire);
            return false;
        }

        var _sirenLigne = "";
        if (!string.IsNullOrEmpty(p_siren))
        {
            _sirenLigne = $"<p style=\"margin: 0.25rem 0;\"><strong>SIREN :</strong> {p_siren}</p>";
        }

        var _corpsHtml = $@"
<!DOCTYPE html>
<html>
<head><meta charset=""utf-8""></head>
<body style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; color: #333;"">
    <div style=""background: #1565C0; color: white; padding: 20px; border-radius: 8px 8px 0 0; text-align: center;"">
        <h1 style=""margin: 0; font-size: 24px;"">🛡️ BTPSecure</h1>
        <p style=""margin: 8px 0 0 0; opacity: 0.9;"">Sécurisation des achats BTP</p>
    </div>

    <div style=""background: #fff; padding: 24px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 8px 8px;"">
        <h2 style=""color: #1565C0; margin-top: 0;"">Une commande a été faite pour votre entreprise</h2>

        <p>Bonjour,</p>

        <p>L'entreprise <strong>{p_nomEntreprisePatron}</strong> vient de créer une commande sur BTPSecure et vous a désigné comme fournisseur.</p>

        <p>Pour visualiser et gérer cette commande, vous devez créer un compte fournisseur sur BTPSecure en utilisant les informations suivantes :</p>

        <div style=""background: #f5f5f5; padding: 16px; border-radius: 6px; margin: 16px 0;"">
            <p style=""margin: 0.25rem 0;""><strong>Nom de l'entreprise :</strong> {p_nomEntrepriseFournisseur}</p>
            <p style=""margin: 0.25rem 0;""><strong>Email associé :</strong> {p_emailFournisseurAssocie}</p>
            <p style=""margin: 0.25rem 0;""><strong>SIRET :</strong> {p_siret}</p>
            {_sirenLigne}
        </div>

        <p style=""margin-top: 24px;"">
            <a href=""{_siteUrl}"" style=""display: inline-block; background: #1565C0; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: 600;"">
                Accéder à BTPSecure
            </a>
        </p>

        <p style=""margin-top: 24px; color: #666; font-size: 14px;"">
            Une fois votre compte créé avec les informations ci-dessus, vous pourrez voir la commande dans la section « Commandes à venir » de votre espace fournisseur.
        </p>

        <hr style=""border: none; border-top: 1px solid #e0e0e0; margin: 24px 0;"">

        <p style=""color: #999; font-size: 12px; text-align: center; margin: 0;"">
            Cet email est envoyé automatiquement par BTPSecure. Ne pas répondre directement.
        </p>
    </div>
</body>
</html>";

        var _payload = new
        {
            sender = new { name = _fromName, email = _fromEmail },
            to = new[] { new { email = p_emailDestinataire } },
            subject = "Nouvelle commande pour votre entreprise sur BTPSecure",
            htmlContent = _corpsHtml
        };

        try
        {
            var _request = new HttpRequestMessage(HttpMethod.Post, "v3/smtp/email")
            {
                Content = JsonContent.Create(_payload)
            };
            _request.Headers.Add("api-key", _apiKey);
            _request.Headers.Add("accept", "application/json");

            var _reponse = await _http.SendAsync(_request);

            if (_reponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email Brevo envoyé à {Email}", p_emailDestinataire);
                return true;
            }
            else
            {
                var _body = await _reponse.Content.ReadAsStringAsync();
                _logger.LogError("Erreur Brevo {Status} pour {Email} : {Body}", _reponse.StatusCode, p_emailDestinataire, _body);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception envoi email à {Email}", p_emailDestinataire);
            return false;
        }
    }
}
