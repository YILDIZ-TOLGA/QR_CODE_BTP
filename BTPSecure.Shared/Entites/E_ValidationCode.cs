namespace BTPSecure.Shared.Entites;

// Trace d'UNE utilisation d'un code, conservée indépendamment du code lui-même.
// Indispensable pour les codes permanents : ils réutilisent la même ligne et
// écrasent leur validation précédente à chaque passage — sans cette table,
// seule la dernière utilisation serait connue.
// Les champs sont des INSTANTANÉS : le code peut être modifié ou régénéré ensuite,
// l'historique doit refléter la situation au moment de la validation.
public class E_ValidationCode
{
    public int Id { get; set; }

    public int CodeId { get; set; }
    public E_Code Code { get; set; } = null!;

    // Permet de filtrer l'historique d'une entreprise sans jointure sur le code
    public int EntrepriseId { get; set; }

    // Porteur du code : collaborateur ou dirigeant. Null si le code visait un tiers externe.
    public int? PorteurId { get; set; }
    public E_Utilisateur? Porteur { get; set; }
    public string? EmailTiers { get; set; }

    // Fournisseur (ou sous-compte fournisseur) qui a validé
    public int ValidateurId { get; set; }
    public E_Utilisateur Validateur { get; set; } = null!;

    public DateTime DateValidation { get; set; }

    // Instantanés au moment de la validation
    public string ValeurUtilisee { get; set; } = string.Empty;
    public string NumeroCommande { get; set; } = string.Empty;
    public int AchatsSupplementaires { get; set; }
    public bool EstPermanent { get; set; }
}
