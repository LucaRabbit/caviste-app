using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using CavisteApp.DTOs.Vins;
using CavisteApp.WPF.Services;
using CavisteApp.WPF.Services.ApiClient;
using CavisteApp.WPF.ViewModels.Editing;
using CavisteApp.WPF.Views;

namespace CavisteApp.WPF.ViewModels;

public class VinsViewModel : ViewModelBase
{
    private readonly IVinsApiClient _vinsApi;
    private readonly SessionService _session;

    private bool _enChargement;
    private VinDto? _vinSelectionne;
    private string _messageErreur = string.Empty;

    public VinsViewModel(IVinsApiClient vinsApi, SessionService session)
    {
        _vinsApi = vinsApi;
        _session = session;

        Vins = new ObservableCollection<VinDto>();
        ChargerCommand = new RelayCommand(ChargerAsync);
        AjouterCommand = new RelayNavCommand(Ajouter,
                                () => EstAdministrateur);
        ModifierCommand = new RelayNavCommand(Modifier,
                                () => VinSelectionne is not null && EstAdministrateur);
        SupprimerCommand = new RelayCommand(SupprimerAsync,
                                () => VinSelectionne is not null && EstAdministrateur);


        // Lancer le chargement en arrière-plan
        _ = ChargerAsync();
    }

    // ─── Propriétés liées à la vue ───────────────────────────────

    public ObservableCollection<VinDto> Vins { get; }

    public VinDto? VinSelectionne
    {
        get => _vinSelectionne;
        set
        {
            if (SetProperty(ref _vinSelectionne, value))
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

            var vins = await _vinsApi.GetTousAsync();

            Vins.Clear();
            foreach (var v in vins) Vins.Add(v);
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
        if (VinSelectionne is null) return;

        var confirmation = MessageBox.Show(
            $"Supprimer le vin '{VinSelectionne.Nom}' ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes) return;

        try
        {
            await _vinsApi.SupprimerAsync(VinSelectionne.Id);
            Vins.Remove(VinSelectionne);
            VinSelectionne = null;
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
        var vm = new VinEditViewModel(_vinsApi);
        if (OuvrirFormulaire(vm) && vm.Resultat is not null)
        {
            Vins.Add(vm.Resultat);
            VinSelectionne = vm.Resultat;
        }
    }

    private void Modifier()
    {
        if (VinSelectionne is null) return;
        var vm = new VinEditViewModel(_vinsApi, VinSelectionne);
        if (OuvrirFormulaire(vm) && vm.Resultat is not null)
        {
            // Remplace dans la collection
            var index = Vins.IndexOf(VinSelectionne);
            if (index >= 0) Vins[index] = vm.Resultat;
            VinSelectionne = vm.Resultat;
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
