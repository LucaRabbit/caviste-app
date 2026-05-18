// ViewModels/NouvelleVenteViewModel.cs
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net.Http;
using System.Windows.Input;
using CavisteApp.Api.Dtos.Ventes;
using CavisteApp.DTOs.Clients;
using CavisteApp.DTOs.Ventes;
using CavisteApp.DTOs.Vins;
using CavisteApp.WPF.Services.ApiClient;

namespace CavisteApp.WPF.ViewModels;

public class NouvelleVenteViewModel : ViewModelBase
{
    private readonly IVentesApiClient _ventesApi;
    private readonly IClientsApiClient _clientsApi;
    private readonly IVinsApiClient _vinsApi;

    private VenteDto? _venteExistante;

    private ClientDto? _clientSelectionne;
    private VinDto? _vinAAjouter;
    private int _quantiteAAjouter = 1;
    private string _messageErreur = string.Empty;
    private bool _enregistrementEnCours;

    public NouvelleVenteViewModel(
        IVentesApiClient ventesApi,
        IClientsApiClient clientsApi,
        IVinsApiClient vinsApi)
    {
        _ventesApi = ventesApi;
        _clientsApi = clientsApi;
        _vinsApi = vinsApi;

        ClientsDisponibles = new ObservableCollection<ClientDto>();
        VinsDisponibles = new ObservableCollection<VinDto>();
        Panier = new ObservableCollection<LignePanierItem>();

        // Quand le panier change, le total se recalcule
        Panier.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(Total));
            (EnregistrerBrouillonCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ValiderCommand as RelayCommand)?.RaiseCanExecuteChanged();
        };

        AjouterLigneCommand = new RelayNavCommand(AjouterLigne, PeutAjouterLigne);
        RetirerLigneCommand = new RelayNavCommand<LignePanierItem>(RetirerLigne);
        EnregistrerBrouillonCommand = new RelayCommand(
            () => EnregistrerAsync(validerImmediatement: false), PeutEnregistrer);
        ValiderCommand = new RelayCommand(
            () => EnregistrerAsync(validerImmediatement: true), PeutEnregistrer);
        AnnulerCommand = new RelayNavCommand(() => Annulee?.Invoke(this, EventArgs.Empty));

        _ = ChargerDonneesAsync();
    }

    // ─── Collections ────────────────────────────────────────
    public ObservableCollection<ClientDto> ClientsDisponibles { get; }
    public ObservableCollection<VinDto> VinsDisponibles { get; }
    public ObservableCollection<LignePanierItem> Panier { get; }

    // ─── Sélections ─────────────────────────────────────────
    public ClientDto? ClientSelectionne
    {
        get => _clientSelectionne;
        set
        {
            if (SetProperty(ref _clientSelectionne, value))
                (EnregistrerBrouillonCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ValiderCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public VinDto? VinAAjouter
    {
        get => _vinAAjouter;
        set
        {
            if (SetProperty(ref _vinAAjouter, value))
            {
                OnPropertyChanged(nameof(StockDisponibleVinAAjouter));
                (AjouterLigneCommand as RelayNavCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public int QuantiteAAjouter
    {
        get => _quantiteAAjouter;
        set
        {
            if (SetProperty(ref _quantiteAAjouter, value))
                (AjouterLigneCommand as RelayNavCommand)?.RaiseCanExecuteChanged();
        }
    }

    public int StockDisponibleVinAAjouter => VinAAjouter?.Stock ?? 0;

    public decimal Total => Panier.Sum(l => l.SousTotal);

    public string MessageErreur
    {
        get => _messageErreur;
        set => SetProperty(ref _messageErreur, value);
    }

    public bool EnregistrementEnCours
    {
        get => _enregistrementEnCours;
        set
        {
            if (SetProperty(ref _enregistrementEnCours, value))
                (EnregistrerBrouillonCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ValiderCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    // ─── Commandes ──────────────────────────────────────────
    public ICommand AjouterLigneCommand { get; }
    public ICommand RetirerLigneCommand { get; }
    public ICommand EnregistrerBrouillonCommand { get; }
    public ICommand ValiderCommand { get; }
    public ICommand AnnulerCommand { get; }

    // ─── Événements (pour MainViewModel) ───────────────────
    public event EventHandler<VenteDto>? VenteValidee;
    public event EventHandler? Annulee;

    // ─── Logique ────────────────────────────────────────────
    private async Task ChargerDonneesAsync()
    {
        try
        {
            var clients = await _clientsApi.GetTousAsync();
            ClientsDisponibles.Clear();
            foreach (var c in clients.OrderBy(c => c.Nom)) ClientsDisponibles.Add(c);

            var vins = await _vinsApi.GetTousAsync();
            VinsDisponibles.Clear();
            foreach (var v in vins.Where(v => v.Stock > 0).OrderBy(v => v.Nom))
                VinsDisponibles.Add(v);
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur de chargement : {ex.Message}";
        }
    }

    private bool PeutAjouterLigne()
        => VinAAjouter is not null
        && QuantiteAAjouter > 0
        && QuantiteAAjouter <= StockDisponibleVinAAjouter;

    private void AjouterLigne()
    {
        if (VinAAjouter is null) return;

        // Si le vin est déjà au panier, on incrémente
        var existante = Panier.FirstOrDefault(l => l.Vin.Id == VinAAjouter.Id);
        if (existante is not null)
        {
            if (existante.Quantite + QuantiteAAjouter > VinAAjouter.Stock)
            {
                MessageErreur = $"Stock insuffisant pour {VinAAjouter.Nom}.";
                return;
            }
            existante.Quantite += QuantiteAAjouter;
            OnPropertyChanged(nameof(Total));
        }
        else
        {
            Panier.Add(new LignePanierItem(VinAAjouter, QuantiteAAjouter));
        }

        QuantiteAAjouter = 1;
        VinAAjouter = null;
        MessageErreur = string.Empty;
    }

    private void RetirerLigne(LignePanierItem? ligne)
    {
        if (ligne is not null) Panier.Remove(ligne);
    }

    public async Task ChargerVenteExistanteAsync(int venteId)
    {
        try
        {
            EnregistrementEnCours = true;
            MessageErreur = string.Empty;

            // Charger les données de base (clients + vins) si pas déjà fait
            if (ClientsDisponibles.Count == 0) await ChargerDonneesAsync();

            // Charger la vente
            _venteExistante = await _ventesApi.GetParIdAsync(venteId);
            if (_venteExistante is null)
            {
                MessageErreur = $"La vente {venteId} est introuvable.";
                return;
            }

            // Restaurer le client sélectionné
            ClientSelectionne = ClientsDisponibles.FirstOrDefault(c => c.Id == _venteExistante.ClientId);

            // Remplir le panier à partir des lignes existantes
            Panier.Clear();
            foreach (var ligne in _venteExistante.Lignes)
            {
                // On retrouve le vin dans la liste des vins disponibles (pour avoir Stock à jour, etc.)
                var vin = VinsDisponibles.FirstOrDefault(v => v.Id == ligne.VinId);
                if (vin is not null)
                {
                    Panier.Add(new LignePanierItem(vin, ligne.Quantite, ligne.Id));
                }
            }

            OnPropertyChanged(nameof(Titre));
            OnPropertyChanged(nameof(EstEdition));
        }
        catch (Exception ex) { MessageErreur = ex.Message; }
        finally { EnregistrementEnCours = false; }
    }

    public bool EstEdition => _venteExistante is not null;
    public string Titre => EstEdition
        ? $"Modifier brouillon n°{_venteExistante!.Id}"
        : "Nouvelle vente";

    private bool PeutEnregistrer()
       => ClientSelectionne is not null
       && Panier.Count > 0
       && !EnregistrementEnCours;

    private async Task EnregistrerAsync(bool validerImmediatement)
    {
        if (!PeutEnregistrer()) return;

        try
        {
            EnregistrementEnCours = true;
            MessageErreur = string.Empty;

            VenteDto vente;

            if (EstEdition)
            {
                // MODE ÉDITION
                var dto = new UpdateVenteDto
                {
                    ClientId = ClientSelectionne!.Id,
                    Lignes = Panier.Select(l => new UpdateLigneVenteDto
                    {
                        Id = l.IdLigneExistante,
                        VinId = l.Vin.Id,
                        Quantite = l.Quantite,
                        PrixUnitaire = l.Vin.Prix
                    }).ToList()
                };

                vente = await _ventesApi.ModifierAsync(_venteExistante!.Id, dto);
            }
            else
            {
                // MODE CRÉATION
                var dto = new CreerVenteDto
                {
                    ClientId = ClientSelectionne!.Id,
                    Lignes = Panier.Select(l => new CreerLigneVenteDto
                    {
                        VinId = l.Vin.Id,
                        Quantite = l.Quantite,
                        PrixUnitaire = l.Vin.Prix
                    }).ToList()
                };

                vente = await _ventesApi.CreerAsync(dto);
            }

            // Validation immédiate si demandée
            if (validerImmediatement)
            {
                vente = await _ventesApi.ValiderAsync(vente.Id);
            }

            VenteValidee?.Invoke(this, vente);
        }
        catch (Exception ex)
        {
            MessageErreur = ex.Message;
        }
        finally
        {
            EnregistrementEnCours = false;
        }
    }

    // Élément du panier — VM léger pour avoir des prix qui s'actualisent
    public class LignePanierItem : ViewModelBase
    {
        private int _quantite;

        public LignePanierItem(VinDto vin, int quantite, int? idExistant = null)
        {
            Vin = vin;
            _quantite = quantite;
            IdLigneExistante = idExistant;
        }

        public VinDto Vin { get; }
        public int? IdLigneExistante { get; }
        public int Quantite
        {
            get => _quantite;
            set
            {
                if (SetProperty(ref _quantite, value))
                    OnPropertyChanged(nameof(SousTotal));
            }
        }
        public decimal SousTotal => Vin.Prix * Quantite;
    }
}