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
}
