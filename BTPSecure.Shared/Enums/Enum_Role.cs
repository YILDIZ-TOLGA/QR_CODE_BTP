namespace BTPSecure.Shared.Enums;

public enum Enum_Role
{
    Admin = 0,
    Dirigeant = 1,
    Collaborateur = 2,
    Fournisseur = 3,
    // Apporteur d'affaires : amène des dirigeants via son code de parrainage
    // et perçoit une commission par filleul validé par un admin.
    ApporteurAffaire = 4
}
