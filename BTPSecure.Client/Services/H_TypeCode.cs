using BTPSecure.Shared.Enums;

namespace BTPSecure.Client.Services;

// Source unique du libellé affiché des types de code
public static class H_TypeCode
{
    public static string Libelle(Enum_TypeCode p_type)
    {
        if (p_type == Enum_TypeCode.LibreService)
        {
            return "Libre-service";
        }
        if (p_type == Enum_TypeCode.Liste)
        {
            return "Liste";
        }
        return p_type.ToString();
    }
}
