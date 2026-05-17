using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using CavisteApp.DTOs.Vins;
using CavisteApp.WPF.Services;
using CavisteApp.WPF.Services.ApiClient;

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
                ((RelayCommand)SupprimerCommand).RaiseCanExecuteChanged();
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
        catch (HttpRequestException ex) when (ex.Message.Contains("403"))
        {
            MessageErreur = "Action réservée à l'administrateur.";
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur : {ex.Message}";
        }
    }
}