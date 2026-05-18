namespace CavisteApp.WPF.ViewModels.Editing;

// Interface commune pour les ViewModels d'édition (ajout/modification)
public interface IEditViewModel
{
    string Titre { get; }
    string MessageErreur { get; }
    bool EnregistrementEnCours { get; }

    // Méthode à implémenter pour valider les données avant l'enregistrement,
    // retourne true si les données sont valides
    // sinon false et MessageErreur doit être renseigné
    Task<bool> EnregistrerAsync();
}
