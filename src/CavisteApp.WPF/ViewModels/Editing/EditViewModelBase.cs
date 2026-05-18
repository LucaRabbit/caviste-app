using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CavisteApp.WPF.ViewModels.Editing;

public abstract class EditViewModelBase : ViewModelBase, IEditViewModel, INotifyDataErrorInfo
{
    private string _messageErreur = string.Empty;
    private bool _enregistrement;
    private readonly Dictionary<string, string> _erreursChamps = new();

    public abstract string Titre { get; }

    public string MessageErreur
    {
        get => _messageErreur;
        protected set => SetProperty(ref _messageErreur, value);
    }

    public bool EnregistrementEnCours
    {
        get => _enregistrement;
        protected set => SetProperty(ref _enregistrement, value);
    }

    // INotifyDataErrorInfo

    public bool HasErrors => _erreursChamps.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    // Retourne les erreurs de validation pour une propriété donnée
    public IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName is null) return Array.Empty<string>();
        return _erreursChamps.TryGetValue(propertyName, out var e)
            ? new[] { e }
            : Array.Empty<string>();
    }

    public bool EstValide => !HasErrors;

    // Méthode utilitaire pour définir les erreurs de validation
    protected void DefinirErreur(string? erreur, [CallerMemberName] string? nomPropriete = null)
    {
        if (nomPropriete is null) return;

        if (string.IsNullOrEmpty(erreur))
            _erreursChamps.Remove(nomPropriete);
        else
            _erreursChamps[nomPropriete] = erreur;

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nomPropriete));
        OnPropertyChanged(nameof(EstValide));
        OnPropertyChanged(nameof(HasErrors));
    }

    public abstract Task<bool> EnregistrerAsync();
}