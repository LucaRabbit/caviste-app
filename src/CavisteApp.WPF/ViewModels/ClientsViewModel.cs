using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using CavisteApp.DTOs.Clients;
using CavisteApp.DTOs.Vins;
using CavisteApp.WPF.Services;
using CavisteApp.WPF.Services.ApiClient;
using CavisteApp.WPF.ViewModels.Editing;
using CavisteApp.WPF.Views;

namespace CavisteApp.WPF.ViewModels;

public class ClientsViewModel : ViewModelBase
{
    private readonly IClientsApiClient _clientsApi;
    private readonly SessionService _session;

    private bool _enChargement;
    private ClientDto? _clientSelectionne;
    private string _messageErreur = string.Empty;

    public ClientsViewModel(IClientsApiClient clientsApi, SessionService session)
    {
        _clientsApi = clientsApi;
        _session = session;

        Clients = new ObservableCollection<ClientDto>();
        ChargerCommand = new RelayCommand(ChargerAsync);
        AjouterCommand = new RelayNavCommand(Ajouter,
                                () => EstAdministrateur);
        ModifierCommand = new RelayNavCommand(Modifier,
                                () => ClientSelectionne is not null && EstAdministrateur);
        SupprimerCommand = new RelayCommand(SupprimerAsync,
                                () => ClientSelectionne is not null && EstAdministrateur);


        // Lancer le chargement en arrière-plan
        _ = ChargerAsync();
    }

    // ─── Propriétés liées à la vue ───────────────────────────────

    public ObservableCollection<ClientDto> Clients { get; }

    public ClientDto? ClientSelectionne
    {
        get => _clientSelectionne;
        set
        {
            if (SetProperty(ref _clientSelectionne, value))
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

            var clients = await _clientsApi.GetTousAsync();

            Clients.Clear();
            foreach (var c in clients) Clients.Add(c);
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
        if (ClientSelectionne is null) return;

        var confirmation = MessageBox.Show(
            $"Supprimer le client '{ClientSelectionne.Nom} {ClientSelectionne.Prenom}' ?",
            "Confirmation",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes) return;

        try
        {
            await _clientsApi.SupprimerAsync(ClientSelectionne.Id);
            Clients.Remove(ClientSelectionne);
            ClientSelectionne = null;
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
        var vm = new ClientEditViewModel(_clientsApi);
        if (OuvrirFormulaire(vm) && vm.Resultat is not null)
        {
            Clients.Add(vm.Resultat);
            ClientSelectionne = vm.Resultat;
        }
    }

    private void Modifier()
    {
        if (ClientSelectionne is null) return;
        var vm = new ClientEditViewModel(_clientsApi, ClientSelectionne);
        if (OuvrirFormulaire(vm) && vm.Resultat is not null)
        {
            // Remplace dans la collection
            var index = Clients.IndexOf(ClientSelectionne);
            if (index >= 0) Clients[index] = vm.Resultat;
            ClientSelectionne = vm.Resultat;
        }
    }

    private bool OuvrirFormulaire(IEditViewModel vm)
    {
        var window = new EditWindow(vm)
        {
            Owner = App.MainAppWindow
        };
        return window.ShowDialog() == true;
    }

}
