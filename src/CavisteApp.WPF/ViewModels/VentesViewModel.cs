using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using CavisteApp.DTOs.Enums;
using CavisteApp.DTOs.Ventes;
using CavisteApp.WPF.Services;
using CavisteApp.WPF.Services.ApiClient;

namespace CavisteApp.WPF.ViewModels;
// ViewModel pour la gestion des ventes : liste, détails, actions (valider, annuler, supprimer)
public class VentesViewModel : ViewModelBase
{
    // Dépendances injectées
    private readonly IVentesApiClient _ventesApi;
    private readonly SessionService _session;
    private readonly Func<NouvelleVenteViewModel> _nouvelleVenteFactory;
    private readonly Action<ViewModelBase> _naviguerVers;

    // États privés
    private VenteResumeDto? _venteSelectionnee;
    private bool _enChargement;
    private string _messageErreur = string.Empty;

    // Filtre de statut (null = "Tous")
    private StatutVente? _filtreStatut;

    // Constructeur avec injection de dépendances
    public VentesViewModel(
        // Injectection des services nécessaires
        IVentesApiClient ventesApi,
        SessionService session,
        Func<NouvelleVenteViewModel> nouvelleVenteFactory,
        Action<ViewModelBase> naviguerVers)
    {
        // Affectation des dépendances
        _ventesApi = ventesApi;
        _session = session;
        _nouvelleVenteFactory = nouvelleVenteFactory;
        _naviguerVers = naviguerVers;

        // Initialisation des collections
        Ventes = new ObservableCollection<VenteResumeDto>();

        // Liste des statuts pour le filtre (null = "Tous")
        StatutsDisponibles = new StatutVente?[] { null }
            .Concat(Enum.GetValues<StatutVente>().Cast<StatutVente?>())
            .ToArray();

        // Commandes initialisées une fois pour toutes
        ChargerCommand = new RelayCommand(ChargerAsync);
        NouvelleVenteCommand = new RelayNavCommand(NouvelleVente);

        ValiderBrouillonCommand = new RelayCommand(ValiderBrouillonAsync, PeutValiderOuSupprimer);
        SupprimerBrouillonCommand = new RelayCommand(SupprimerBrouillonAsync, PeutValiderOuSupprimer);
        AnnulerVenteCommand = new RelayCommand(AnnulerVenteAsync, PeutAnnuler);
        ModifierBrouillonCommand = new RelayCommand(ModifierBrouillonAsync, PeutValiderOuSupprimer);
        EffacerFiltreCommand = new RelayNavCommand(() => FiltreStatut = null);

        // Chargement initial des ventes
        _ = ChargerAsync();
    }

    // Commandes exposées au XAML
    public ICommand ChargerCommand { get; }
    public ICommand NouvelleVenteCommand { get; }
    public ICommand ValiderBrouillonCommand { get; }
    public ICommand SupprimerBrouillonCommand { get; }
    public ICommand AnnulerVenteCommand { get; }
    public ICommand ModifierBrouillonCommand { get; }
    public ICommand EffacerFiltreCommand { get; }

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

                // Charger les détails
                _ = ChargerDetailsAsync(value?.Id);
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

    // Détails de la vente sélectionnée (chargés à la demande)
    private VenteDto? _venteDetails;

    public VenteDto? VenteDetails
    {
        get => _venteDetails;
        private set => SetProperty(ref _venteDetails, value);
    }

    // Logique

    // CanExecute des nouvelles commandes
    private bool PeutValiderOuSupprimer() => VenteEstBrouillon;
    private bool PeutAnnuler() => VenteEstValidee && EstAdministrateur;

    // Chargement des ventes
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

    // Chargement à la demande des détails de la vente sélectionnée
    private async Task ChargerDetailsAsync(int? venteId)
    {
        if (venteId is null)
        {
            VenteDetails = null;
            return;
        }

        try
        {
            VenteDetails = await _ventesApi.GetParIdAsync(venteId.Value);
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur chargement détails : {ex.Message}";
            VenteDetails = null;
        }
    }

    // Navigation vers la création d'une nouvelle vente
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


    // Actions sur les ventes
    // Valider un brouillon, modifier un brouillon, supprimer un brouillon, annuler une vente validée
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

    // Annuler une vente validée (administrateur)
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