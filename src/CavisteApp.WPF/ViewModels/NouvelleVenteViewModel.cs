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
            OnPropertyChanged(nameof(StockDisponibleVinAAjouter));
            OnPropertyChanged(nameof(QuantiteDepasseStock));
            OnPropertyChanged(nameof(PanierIncoherent));
            OnPropertyChanged(nameof(MessageCoherence));
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
    // Client sélectionné pour la vente (lié à une liste de sélection dans la vue)
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

    // Vin sélectionné pour ajout au panier (lié à une liste de sélection dans la vue)
    public VinDto? VinAAjouter
    {
        get => _vinAAjouter;
        set
        {
            if (SetProperty(ref _vinAAjouter, value))
            {
                OnPropertyChanged(nameof(StockDisponibleVinAAjouter));
                OnPropertyChanged(nameof(QuantiteDepasseStock));
                (AjouterLigneCommand as RelayNavCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    // Quantité à ajouter pour le vin sélectionné (lié à un champ de saisie dans la vue)
    public int QuantiteAAjouter
    {
        get => _quantiteAAjouter;
        set
        {
            if (SetProperty(ref _quantiteAAjouter, value))
                OnPropertyChanged(nameof(QuantiteDepasseStock));
                (AjouterLigneCommand as RelayNavCommand)?.RaiseCanExecuteChanged();
        }
    }

    // ─── Calculs ─────────────────────────────────────────────
    // Calcule le total de la vente en sommant les sous-totaux de chaque ligne du panier
    public decimal Total => Panier.Sum(l => l.SousTotal);

    // Calcule le stock disponible pour le vin sélectionné en tenant compte de ce qui est déjà dans le panier
    public int StockDisponibleVinAAjouter
    {
        get
        {
            if (VinAAjouter is null) return 0;
            var dejaAuPanier = Panier
                .Where(l => l.Vin.Id == VinAAjouter.Id)
                .Sum(l => l.Quantite);
            return VinAAjouter.Stock - dejaAuPanier;
        }
    }

    // Message d'erreur à afficher dans la vue (lié à des problèmes de validation ou d'API)
    public string MessageErreur
    {
        get => _messageErreur;
        set => SetProperty(ref _messageErreur, value);
    }

    // Indique si une opération d'enregistrement est en cours (pour désactiver les boutons et éviter les doubles clics)
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
    // Charge les clients et vins disponibles (avec stock > 0) pour les listes de sélection
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

    // Vérifie que la ligne à ajouter est valide (vin sélectionné, quantité positive et pas supérieure au stock disponible)
    private bool PeutAjouterLigne()
        => VinAAjouter is not null
        && QuantiteAAjouter > 0
        && QuantiteAAjouter <= StockDisponibleVinAAjouter;

    // Ajoute une ligne au panier (ou incrémente la quantité si le vin est déjà présent)
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
            OnPropertyChanged(nameof(PanierIncoherent));
            OnPropertyChanged(nameof(MessageCoherence));
        }
        else
        {
            Panier.Add(new LignePanierItem(VinAAjouter, QuantiteAAjouter));
        }

        QuantiteAAjouter = 1;
        VinAAjouter = null;
        MessageErreur = string.Empty;
    }

    // Retire une ligne du panier
    private void RetirerLigne(LignePanierItem? ligne)
    {
        if (ligne is not null) Panier.Remove(ligne);
    }

    // Charge une vente existante pour modification (brouillon)
    public async Task ChargerVenteExistanteAsync(int venteId)
    {
        try
        {
            EnregistrementEnCours = true;
            MessageErreur = string.Empty;

            // Charger les données de base
            await ChargerDonneesAsync();

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
            OnPropertyChanged(nameof(PanierIncoherent));
            OnPropertyChanged(nameof(MessageCoherence));
        }
        catch (Exception ex) { MessageErreur = ex.Message; }
        finally { EnregistrementEnCours = false; }
    }

    // Indique si on est en train de modifier une vente existante (true) ou d'en créer une nouvelle (false)
    public bool EstEdition => _venteExistante is not null;
    public string Titre => EstEdition
        ? $"Modifier brouillon n°{_venteExistante!.Id}"
        : "Nouvelle vente";

    // Conditions pour pouvoir enregistrer (brouillon ou validation)
    private bool PeutEnregistrer()
       => ClientSelectionne is not null
       && Panier.Count > 0
       && !EnregistrementEnCours;

    // Vérifie que la quantité à ajouter ne dépasse pas le stock disponible (en tenant compte du panier actuel)
    public bool QuantiteDepasseStock =>
    VinAAjouter is not null
    && QuantiteAAjouter > StockDisponibleVinAAjouter;

    // Vérifie que toutes les lignes du panier sont cohérentes avec le stock actuel (en cas de modification d'une vente existante)
    public bool PanierIncoherent => Panier.Any(l => l.Quantite > l.Vin.Stock);

    // Message détaillé des problèmes de stock pour chaque ligne incohérente
    public string MessageCoherence
    {
        get
        {
            var problemes = Panier.Where(l => l.Quantite > l.Vin.Stock).ToList();
            if (problemes.Count == 0) return string.Empty;

            var lignes = problemes.Select(l =>
                $"• {l.Vin.Nom} : {l.Quantite} demandés, {l.Vin.Stock} disponibles");
            return "Stock insuffisant :\n" + string.Join("\n", lignes);
        }
    }

    // Enregistre la vente en brouillon ou la valide immédiatement selon le paramètre
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