// ViewModels/CommandesViewModel.cs
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CavisteApp.DTOs.Commandes;
using CavisteApp.DTOs.Enums;
using CavisteApp.WPF.Services;
using CavisteApp.WPF.Services.ApiClient;

namespace CavisteApp.WPF.ViewModels;

public class CommandesViewModel : ViewModelBase
{
    private readonly ICommandesApiClient _commandesApi;
    private readonly SessionService _session;
    private readonly Func<NouvelleCommandeViewModel> _nouvelleCommandeFactory;
    private readonly Action<ViewModelBase> _naviguerVers;

    private CommandeResumeDto? _commandeSelectionnee;
    private bool _enChargement;
    private string _messageErreur = string.Empty;

    private StatutCommande? _filtreStatut;

    public CommandesViewModel(
        ICommandesApiClient commandesApi,
        SessionService session,
        Func<NouvelleCommandeViewModel> nouvelleCommandeFactory,
        Action<ViewModelBase> naviguerVers)
    {
        _commandesApi = commandesApi;
        _session = session;
        _nouvelleCommandeFactory = nouvelleCommandeFactory;
        _naviguerVers = naviguerVers;

        Commandes = new ObservableCollection<CommandeResumeDto>();

        StatutsDisponibles = new StatutCommande?[] { null }
            .Concat(Enum.GetValues<StatutCommande>().Cast<StatutCommande?>())
            .ToArray();

        ChargerCommand = new RelayCommand(ChargerAsync);
        NouvelleCommandeCommand = new RelayNavCommand(NouvelleCommande);

        ModifierBrouillonCommand = new RelayCommand(ModifierBrouillonAsync, PeutValiderOuSupprimer);
        SupprimerBrouillonCommand = new RelayCommand(SupprimerBrouillonAsync, PeutValiderOuSupprimer);
        ValiderBrouillonCommand = new RelayCommand(ValiderBrouillonAsync, PeutValiderOuSupprimer);
        ReceptionnerCommand = new RelayCommand(ReceptionnerAsync, () => CommandeEstValidee);
        AnnulerCommandeCommand = new RelayCommand(AnnulerCommandeAsync, PeutAnnuler);
        EffacerFiltreCommand = new RelayNavCommand(() => FiltreStatut = null);

        _ = ChargerAsync();
    }

    // 
    public ICommand ChargerCommand { get; }
    public ICommand NouvelleCommandeCommand { get; }
    public ICommand ModifierBrouillonCommand { get; }
    public ICommand SupprimerBrouillonCommand { get; }
    public ICommand ValiderBrouillonCommand { get; }
    public ICommand ReceptionnerCommand { get; }
    public ICommand AnnulerCommandeCommand { get; }
    public ICommand EffacerFiltreCommand { get; }


    public bool CommandeEstBrouillon => CommandeSelectionnee?.Statut == StatutCommande.Brouillon;
    public bool CommandeEstValidee => CommandeSelectionnee?.Statut == StatutCommande.Validee;
    public bool EstAdministrateur => _session.IsAdmin;

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


    public ObservableCollection<CommandeResumeDto> Commandes { get; }
    public StatutCommande?[] StatutsDisponibles { get; }


    public CommandeResumeDto? CommandeSelectionnee
    {
        get => _commandeSelectionnee;
        set
        {
            if (SetProperty(ref _commandeSelectionnee, value))
            {
                // Notifie les bool calculés
                OnPropertyChanged(nameof(CommandeEstBrouillon));
                OnPropertyChanged(nameof(CommandeEstValidee));

                NotifierCommandes();
                _ = ChargerDetailsAsync(value?.Id);
            }
        }
    }

    // Détails de la commande sélectionnée (chargés à la demande)
    private CommandeDto? _commandeDetails;

    public CommandeDto? CommandeDetails
    {
        get => _commandeDetails;
        private set => SetProperty(ref _commandeDetails, value);
    }

    public StatutCommande? FiltreStatut
    {
        get => _filtreStatut;
        set
        {
            if (SetProperty(ref _filtreStatut, value))
                _ = ChargerAsync();
        }
    }

    // Logique

    // CanExecute des nouvelles commandes
    private bool PeutValiderOuSupprimer() => CommandeEstBrouillon;
    private bool PeutAnnuler() => CommandeEstValidee && EstAdministrateur;

    private void NotifierCommandes()
    {
        (ModifierBrouillonCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (SupprimerBrouillonCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ValiderBrouillonCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ReceptionnerCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (AnnulerCommandeCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    private async Task ChargerAsync()
    {
        try
        {
            EnChargement = true;
            MessageErreur = string.Empty;
            var commandes = await _commandesApi.GetTousAsync(FiltreStatut);
            Commandes.Clear();
            foreach (var c in commandes) Commandes.Add(c);
        }
        catch (Exception ex) { MessageErreur = $"Erreur : {ex.Message}"; }
        finally { EnChargement = false; }
    }

    private async Task ChargerDetailsAsync(int? id)
    {
        if (id is null) { CommandeDetails = null; return; }
        try { CommandeDetails = await _commandesApi.GetParIdAsync(id.Value); }
        catch (Exception ex) { MessageErreur = ex.Message; CommandeDetails = null; }
    }

    private void NouvelleCommande()
    {
        var vm = _nouvelleCommandeFactory();
        vm.CommandeEnregistree += async (s, c) =>
        {
            _naviguerVers(this);
            await ChargerAsync();
        };
        vm.Annulee += (s, e) => _naviguerVers(this);
        _naviguerVers(vm);
    }

    private async Task ModifierBrouillonAsync()
    {
        if (CommandeSelectionnee is null) return;
        var vm = _nouvelleCommandeFactory();
        vm.CommandeEnregistree += async (s, c) =>
        {
            _naviguerVers(this);
            await ChargerAsync();
        };
        vm.Annulee += (s, e) => _naviguerVers(this);
        _naviguerVers(vm);
        await vm.ChargerCommandeExistanteAsync(CommandeSelectionnee.Id);
    }

    private async Task ValiderBrouillonAsync()
    {
        if (CommandeSelectionnee is null) return;
        try { await _commandesApi.ValiderAsync(CommandeSelectionnee.Id); await ChargerAsync(); }
        catch (Exception ex) { MessageErreur = ex.Message; }
    }

    private async Task ReceptionnerAsync()
    {
        if (CommandeSelectionnee is null) return;

        var confirm = MessageBox.Show(
            $"Marquer la commande n°{CommandeSelectionnee.Id} comme reçue ? Le stock sera mis à jour.",
            "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Information);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            // Charger la commande complète pour récupérer ses lignes
            var commande = await _commandesApi.GetParIdAsync(CommandeSelectionnee.Id);
            if (commande is null)
            {
                MessageErreur = "Commande introuvable.";
                return;
            }

            // Construire le DTO de réception : on reçoit tout ce qui a été commandé
            var dto = new ReceptionnerCommandeDto
            {
                Lignes = commande.Lignes.Select(l => new ReceptionLigneDto
                {
                    Id = l.Id,
                    QuantiteRecue = l.Quantite
                }).ToList()
            };

            await _commandesApi.ReceptionnerAsync(CommandeSelectionnee.Id, dto);
            await ChargerAsync();
        }
        catch (Exception ex) { MessageErreur = ex.Message; }
    }


    private async Task AnnulerCommandeAsync()
    {
        if (CommandeSelectionnee is null) return;

        var confirm = MessageBox.Show(
            $"Annuler la commande n°{CommandeSelectionnee.Id} ?",
            "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            await _commandesApi.AnnulerAsync(CommandeSelectionnee.Id,
                new AnnulerCommandeDto { Motif = "Annulée par l'administrateur" });
            await ChargerAsync();
        }
        catch (Exception ex) { MessageErreur = ex.Message; }
    }

    private async Task SupprimerBrouillonAsync()
    {
        if (CommandeSelectionnee is null) return;
        var confirm = MessageBox.Show(
            $"Supprimer le brouillon n°{CommandeSelectionnee.Id} ?",
            "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            await _commandesApi.SupprimerAsync(CommandeSelectionnee.Id);
            Commandes.Remove(CommandeSelectionnee);
            CommandeSelectionnee = null;
        }
        catch (Exception ex) { MessageErreur = ex.Message; }
    }
}