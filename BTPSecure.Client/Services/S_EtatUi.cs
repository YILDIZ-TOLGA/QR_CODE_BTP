namespace BTPSecure.Client.Services;

// Petit bus d'évènements pour que les pages puissent demander au layout
// de rafraîchir ses compteurs (badges sidebar) après une action.
// Évite d'ajouter un polling supplémentaire juste pour les badges.
public class S_EtatUi
{
    public event Action? CompteursChanges;

    public void SignalerChangement()
    {
        CompteursChanges?.Invoke();
    }
}
