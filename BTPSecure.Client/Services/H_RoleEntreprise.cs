using BTPSecure.Shared.Enums;
using Radzen;

namespace BTPSecure.Client.Services;

// Source UNIQUE des libellés et couleurs des rôles internes à l'entreprise.
// Réutilisée partout dans l'app (dashboard, profil, etc.).
public static class H_RoleEntreprise
{
    public static string Libelle(Enum_RoleEntreprise p_role)
    {
        if (p_role == Enum_RoleEntreprise.Responsable)
        {
            return "Responsable";
        }
        if (p_role == Enum_RoleEntreprise.ResponsableAdmin)
        {
            return "Responsable Admin";
        }
        return "Collaborateur";
    }

    public static BadgeStyle Style(Enum_RoleEntreprise p_role)
    {
        if (p_role == Enum_RoleEntreprise.Responsable)
        {
            return BadgeStyle.Info;
        }
        if (p_role == Enum_RoleEntreprise.ResponsableAdmin)
        {
            return BadgeStyle.Warning;
        }
        return BadgeStyle.Light;
    }

    public static string Icone(Enum_RoleEntreprise p_role)
    {
        if (p_role == Enum_RoleEntreprise.Responsable)
        {
            return "shield";
        }
        if (p_role == Enum_RoleEntreprise.ResponsableAdmin)
        {
            return "admin_panel_settings";
        }
        return "person";
    }

    // Couleur d'accent (bordure de carte, icône de section) — alignée sur Style()
    public static string Couleur(Enum_RoleEntreprise p_role)
    {
        if (p_role == Enum_RoleEntreprise.Responsable)
        {
            return "var(--rz-info)";
        }
        if (p_role == Enum_RoleEntreprise.ResponsableAdmin)
        {
            return "var(--rz-warning)";
        }
        return "var(--rz-base-400)";
    }

    // Libellé au pluriel, pour les en-têtes de section
    public static string LibellePluriel(Enum_RoleEntreprise p_role)
    {
        if (p_role == Enum_RoleEntreprise.Responsable)
        {
            return "Responsables";
        }
        if (p_role == Enum_RoleEntreprise.ResponsableAdmin)
        {
            return "Responsables Admin";
        }
        return "Collaborateurs";
    }

    // Rappel des droits du rôle, affiché sous l'en-tête de section
    public static string Description(Enum_RoleEntreprise p_role)
    {
        if (p_role == Enum_RoleEntreprise.Responsable)
        {
            return "Code permanent libre-service, accès H24 (régénéré à chaque utilisation).";
        }
        if (p_role == Enum_RoleEntreprise.ResponsableAdmin)
        {
            return "Bras droit : code permanent libre-service + peut créer des codes pour l'entreprise.";
        }
        return "Reçoit des codes ponctuels créés par le dirigeant.";
    }
}
