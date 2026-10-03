using BTPSecure.Shared.Enums;

namespace BTPSecure.Shared.Entites;

// Lien entre un apporteur d'affaires et un dirigeant qu'il a amené dans l'application.
// Créé à l'inscription du filleul (statut EnAttente), puis validé par un admin au moment
// où il autorise l'entreprise du filleul — c'est à cet instant, et seulement là, que la
// commission est fixée.
// ⚠️ MontantCommission n'est JAMAIS recalculé après coup : l'autorisation d'une entreprise
// est une bascule, et un parrainage déjà validé ne doit pas être repayé si l'admin bloque
// puis réautorise.
public class E_Parrainage
{
    public int Id { get; set; }

    // L'apporteur qui encaisse
    public int ApporteurId { get; set; }
    public E_Utilisateur Apporteur { get; set; } = null!;

    // Le dirigeant amené. Un filleul n'est parrainé qu'une seule fois (index unique).
    public int FilleulId { get; set; }
    public E_Utilisateur Filleul { get; set; } = null!;

    // Instantané du code saisi : l'apporteur pourrait en changer, la trace doit rester lisible
    public string CodeUtilise { get; set; } = string.Empty;

    public DateTime DateInscription { get; set; }

    public Enum_StatutParrainage Statut { get; set; } = Enum_StatutParrainage.EnAttente;

    // Fixée par l'admin à la validation ; 0 tant que le parrainage est en attente
    public decimal MontantCommission { get; set; }

    public DateTime? DateValidation { get; set; }

    // Admin qui a validé et fixé le montant
    public int? ValidateurId { get; set; }
    public E_Utilisateur? Validateur { get; set; }
}
