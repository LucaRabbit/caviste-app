using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using CavisteApp.DTOs.Enums;
using CavisteApp.DTOs.Ventes;
using CavisteApp.WPF.Services;
using CavisteApp.WPF.Services.ApiClient;

namespace CavisteApp.WPF.ViewModels;

public class VentesViewModel : ViewModelBase
{
    private readonly IVentesApiClient _ventesApi;
    private readonly SessionService _session;
    private readonly Func<NouvelleVenteViewModel> _nouvelleVenteFactory;
    private readonly Action<ViewModelBase> _naviguerVers;

    private VenteResumeDto? _venteSelectionnee;
    private bool _enChargement;
    private string _messageErreur = string.Empty;

    private StatutVente? _filtreStatut;

    public VentesViewModel(
        IVentesApiClient ventesApi,
        SessionService session,
        Func<NouvelleVenteViewModel> nouvelleVenteFactory,
        Action<ViewModelBase> naviguerVers)
    {
        _ventesApi = ventesApi;
        _session = session;
        _nouvelleVenteFactory = nouvelleVenteFactory;
        _naviguerVers = naviguerVers;

        Ventes = new ObservableCollection<VenteResumeDto>();

        // Liste des statuts pour le filtre (null = "Tous")
        StatutsDisponibles = new StatutVente?[] { null }
            .Concat(Enum.GetValues<StatutVente>().Cast<StatutVente?>())
            .ToArray();

        ChargerCommand = new RelayCommand(ChargerAsync);
        NouvelleVenteCommand = new RelayNavCommand(NouvelleVente);

        ValiderBrouillonCommand = new RelayCommand(ValiderBrouillonAsync, PeutValiderOuSupprimer);
        SupprimerBrouillonCommand = new RelayCommand(SupprimerBrouillonAsync, PeutValiderOuSupprimer);
        AnnulerVenteCommand = new RelayCommand(AnnulerVenteAsync, PeutAnnuler);
        ModifierBrouillonCommand = new RelayCommand(ModifierBrouillonAsync, PeutValiderOuSupprimer);

        _ = ChargerAsync();
    }

    // Collections
    public ObservableCollection<VenteResumeDto> Ventes { get; }
    public StatutVente?[] StatutsDisponibles { get; }

    // Selection
    public VenteResumeDto? VenteSelectionnee
    {
        get => _venteSelectionnee;
        set
        {
            if (SetProperty(ref _venteSelectionnee, value))
            {
                // Notifie les bool calculés
                OnPropertyChanged(nameof(VenteEstBrouillon));
                OnPropertyChanged(nameof(VenteEstValidee));

                (ValiderBrouillonCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (SupprimerBrouillonCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (AnnulerVenteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ModifierBrouillonCommand as RelayCommand)?.RaiseCanExecuteChanged();

            }
        }
    }

    // Filtre
    public StatutVente? FiltreStatut
    {
        get => _filtreStatut;
        set
        {
            if (SetProperty(ref _filtreStatut, value))
                _ = ChargerAsync();
        }
    }

    // États dérivés (utilisés par le XAML et les CanExecute)
    public bool VenteEstBrouillon => VenteSelectionnee?.Statut == StatutVente.Brouillon;
    public bool VenteEstValidee => VenteSelectionnee?.Statut == StatutVente.Validee;
    public bool EstAdministrateur => _session.IsAdmin;

    // Indicateurs
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

    // Commandes
    public ICommand ChargerCommand { get; }
    public ICommand NouvelleVenteCommand { get; }
    public ICommand ValiderBrouillonCommand { get; }
    public ICommand SupprimerBrouillonCommand { get; }
    public ICommand AnnulerVenteCommand { get; }
    public ICommand ModifierBrouillonCommand { get; }

    // Logique

    private async Task ChargerAsync()
    {
        try
        {
            EnChargement = true;
            MessageErreur = string.Empty;
            var ventes = await _ventesApi.GetTousAsync(FiltreStatut);
            Ventes.Clear();
            foreach (var v in ventes) Ventes.Add(v);
        }
        catch (Exception ex) { MessageErreur = $"Erreur : {ex.Message}"; }
        finally { EnChargement = false; }
    }

    private void NouvelleVente()
    {
        var vm = _nouvelleVenteFactory();
        vm.VenteValidee += async (s, vente) =>
        {
            _naviguerVers(this);
            await ChargerAsync();
        };
        vm.Annulee += (s, e) => _naviguerVers(this);
        _naviguerVers(vm);
    }

    // CanExecute des nouvelles commandes
    private bool PeutValiderOuSupprimer() => VenteEstBrouillon;
    private bool PeutAnnuler() => VenteEstValidee && EstAdministrateur;

    // Actions sur les ventes
    private async Task ValiderBrouillonAsync()
    {
        if (VenteSelectionnee is null) return;

        try
        {
            await _ventesApi.ValiderAsync(VenteSelectionnee.Id);
            await ChargerAsync();
        }
        catch (Exception ex) { MessageErreur = ex.Message; }
    }
    private async Task ModifierBrouillonAsync()
    {
        if (VenteSelectionnee is null) return;

        var vm = _nouvelleVenteFactory();
        vm.VenteValidee += async (s, vente) =>
        {
            _naviguerVers(this);
            await ChargerAsync();
        };
        vm.Annulee += (s, e) => _naviguerVers(this);

        // Bascule la vue d'abord, puis charge
        _naviguerVers(vm);
        await vm.ChargerVenteExistanteAsync(VenteSelectionnee.Id);
    }

    private async Task SupprimerBrouillonAsync()
    {
        if (VenteSelectionnee is null) return;

        var confirm = MessageBox.Show(
            $"Supprimer le brouillon n°{VenteSelectionnee.Id} ?",
            "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            await _ventesApi.SupprimerAsync(VenteSelectionnee.Id);
            Ventes.Remove(VenteSelectionnee);
            VenteSelectionnee = null;
        }
        catch (Exception ex) { MessageErreur = ex.Message; }
    }

    private async Task AnnulerVenteAsync()
    {
        if (VenteSelectionnee is null) return;

        var confirm = MessageBox.Show(
            $"Annuler la vente n°{VenteSelectionnee.Id} ? Le stock sera restauré.",
            "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            await _ventesApi.AnnulerAsync(VenteSelectionnee.Id,
                new AnnulerVenteDto { Motif = "Annulée par l'administrateur" });
            await ChargerAsync();
        }
        catch (Exception ex) { MessageErreur = ex.Message; }
    }
}