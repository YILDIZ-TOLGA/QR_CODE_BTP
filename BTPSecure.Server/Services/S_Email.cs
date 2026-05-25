using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BTPSecure.Server.Services;

public class S_Email
{
    private readonly IConfiguration _config;
    private readonly ILogger<S_Email> _logger;

    public S_Email(IConfiguration p_config, ILogger<S_Email> p_logger)
    {
        _config = p_config;
        _logger = p_logger;
    }

    public async Task<bool> EnvoyerInvitationFournisseur(string p_emailDestinataire, string p_nomEntreprisePatron,
        string p_nomEntrepriseFournisseur, string p_siret, string? p_siren, string p_emailFournisseurAssocie)
    {
        var _host = Environment.GetEnvironmentVariable("SMTP_HOST") ?? _config["Smtp:Host"];
        var _portStr = Environment.GetEnvironmentVariable("SMTP_PORT") ?? _config["Smtp:Port"];
        var _user = Environment.GetEnvironmentVariable("SMTP_USER") ?? _config["Smtp:User"];
        var _key = Environment.GetEnvironmentVariable("SMTP_KEY") ?? _config["Smtp:Key"];
        var _from = Environment.GetEnvironmentVariable("SMTP_FROM") ?? _config["Smtp:From"];
        var _siteUrl = Environment.GetEnvironmentVariable("SITE_URL") ?? _config["Site:Url"] ?? "https://www.codebtpsecure.cloud";

        if (string.IsNullOrEmpty(_host) || string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_key) || string.IsNullOrEmpty(_from))
        {
            _logger.LogWarning("Configuration SMTP manquante, email non envoyé à {Email}", p_emailDestinataire);
            return false;
        }

        if (!int.TryParse(_portStr, out var _port))
            _port = 587;

        var _sirenLigne = "";
        if (!string.IsNullOrEmpty(p_siren))
        {
            _sirenLigne = $"<p style=\"margin: 0.25rem 0;\"><strong>SIREN :</strong> {p_siren}</p>";
        }

        var _sujet = $"Nouvelle commande pour votre entreprise sur BTPSecure";

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

        try
        {
            var _message = new MimeMessage();
            _message.From.Add(MailboxAddress.Parse(_from));
            _message.To.Add(MailboxAddress.Parse(p_emailDestinataire));
            _message.Subject = _sujet;

            var _builder = new BodyBuilder { HtmlBody = _corpsHtml };
            _message.Body = _builder.ToMessageBody();

            using var _client = new SmtpClient();
            await _client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
            await _client.AuthenticateAsync(_user, _key);
            await _client.SendAsync(_message);
            await _client.DisconnectAsync(true);

            _logger.LogInformation("Email d'invitation envoyé à {Email}", p_emailDestinataire);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur envoi email à {Email}", p_emailDestinataire);
            return false;
        }
    }
}
