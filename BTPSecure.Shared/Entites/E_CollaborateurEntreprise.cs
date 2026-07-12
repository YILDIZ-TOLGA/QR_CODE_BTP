using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.Entites;

public class E_CollaborateurEntreprise
{
    public int Id { get; set; }
    public int CollaborateurId { get; set; }
    public E_Utilisateur Collaborateur { get; set; } = null!;
    public int EntrepriseId { get; set; }
    public E_Entreprise Entreprise { get; set; } = null!;
    public DateTime DateAjout { get; set; }
    public bool EstActif { get; set; } = true;
    public Enum_StatutInvitation StatutInvitation { get; set; } = Enum_StatutInvitation.EnAttente;
}
