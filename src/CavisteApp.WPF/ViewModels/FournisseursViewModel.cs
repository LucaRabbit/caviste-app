using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using CavisteApp.DTOs.Fournisseurs;
using CavisteApp.WPF.Services;
using CavisteApp.WPF.Services.ApiClient;
using CavisteApp.WPF.ViewModels.Editing;
using CavisteApp.WPF.Views;

namespace CavisteApp.WPF.ViewModels;

public class FournisseursViewModel : ViewModelBase
{
    private readonly IFournisseursApiClient _fournisseursApi;
    private readonly SessionService _session;

    private bool _enChargement;
    private FournisseurDto? _fournisseurSelectionne;
    private string _messageErreur = string.Empty;

    public FournisseursViewModel(IFournisseursApiClient fournisseursApi, SessionService session)
    {
        _fournisseursApi = fournisseursApi;
        _session = session;

        Fournisseurs = new ObservableCollection<FournisseurDto>();
        ChargerCommand = new RelayCommand(ChargerAsync);
        AjouterCommand = new RelayNavCommand(Ajouter,
                                () => EstAdministrateur);
        ModifierCommand = new RelayNavCommand(Modifier,
                                () => FournisseurSelectionne is not null && EstAdministrateur);
        SupprimerCommand = new RelayCommand(SupprimerAsync,
                                () => FournisseurSelectionne is not null && EstAdministrateur);

        // Lancer le chargement en arrière-plan
        _ = ChargerAsync();
    }

    // ─── Propriétés liées à la vue ───────────────────────────────

    public ObservableCollection<FournisseurDto> Fournisseurs { get; }

    public FournisseurDto? FournisseurSelectionne
    {
        get => _fournisseurSelectionne;
        set
        {
            if (SetProperty(ref _fournisseurSelectionne, value))
            {
                (ModifierCommand as RelayNavCommand)?.RaiseCanExecuteChanged();
                (SupprimerCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool EnChargement
    {
        get => _enChargement;
        set => SetProperty(ref _enChargement, value);
    }

    public string MessageErreur
    {
        get => _messageErreur;
        set => SetProperty(ref _messageErreur, value);
    }

    public bool EstAdministrateur => _session.IsAdmin;

    // ─── Commandes ──────────────────────────────────────────────

    public ICommand ChargerCommand { get; }
    public ICommand AjouterCommand { get; }
    public ICommand ModifierCommand { get; }
    public ICommand SupprimerCommand { get; }

    // ─── Logique ────────────────────────────────────────────────

    private async Task ChargerAsync()
    {
        try
        {
            EnChargement = true;
            MessageErreur = string.Empty;

            var fournisseurs = await _fournisseursApi.GetTousAsync();

            Fournisseurs.Clear();
            foreach (var f in fournisseurs) Fournisseurs.Add(f);
        }
        catch (HttpRequestException ex)
        {
            MessageErreur = $"Erreur réseau : {ex.Message}";
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur : {ex.Message}";
        }
        finally
        {
            EnChargement = false;
        }
    }

    private async Task SupprimerAsync()
    {
        if (FournisseurSelectionne is null) return;

        var confirmation = MessageBox.Show(
            $"Supprimer le fournisseur '{FournisseurSelectionne.Nom}' ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes) return;

        try
        {
            await _fournisseursApi.SupprimerAsync(FournisseurSelectionne.Id);
            Fournisseurs.Remove(FournisseurSelectionne);
            FournisseurSelectionne = null;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("409"))
        {
            ApiErrorHelper.AfficherErreur(ex, "Suppression impossible");
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("403"))
        {
            MessageErreur = "Action réservée à l'administrateur.";
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur : {ex.Message}";
        }
    }

    private void Ajouter()
    {
        var vm = new FournisseurEditViewModel(_fournisseursApi);
        if (OuvrirFormulaire(vm) && vm.Resultat is not null)
        {
            Fournisseurs.Add(vm.Resultat);
            FournisseurSelectionne = vm.Resultat;
        }
    }

    private void Modifier()
    {
        if (FournisseurSelectionne is null) return;
        var vm = new FournisseurEditViewModel(_fournisseursApi, FournisseurSelectionne);
        if (OuvrirFormulaire(vm) && vm.Resultat is not null)
        {
            // Remplace dans la collection
            var index = Fournisseurs.IndexOf(FournisseurSelectionne);
            if (index >= 0) Fournisseurs[index] = vm.Resultat;
            FournisseurSelectionne = vm.Resultat;
        }
    }

    private bool OuvrirFormulaire(IEditViewModel vm)
    {
        var window = new EditWindow(vm)
        {
            Owner = Application.Current.MainWindow
        };
        return window.ShowDialog() == true;
    }
}