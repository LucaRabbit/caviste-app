// ViewModels/NouvelleCommandeViewModel.cs
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Windows.Input;
using CavisteApp.DTOs.Commandes;
using CavisteApp.DTOs.Fournisseurs;
using CavisteApp.DTOs.Vins;
using CavisteApp.WPF.Services.ApiClient;

namespace CavisteApp.WPF.ViewModels;

public class NouvelleCommandeViewModel : ViewModelBase
{
    private readonly ICommandesApiClient _commandesApi;
    private readonly IFournisseursApiClient _fournisseursApi;
    private readonly IVinsApiClient _vinsApi;

    private CommandeDto? _commandeExistante;
    private FournisseurDto? _fournisseurSelectionne;
    private VinDto? _vinAAjouter;
    private int _quantiteAAjouter = 1;
    private string _messageErreur = string.Empty;
    private bool _enregistrementEnCours;

    public NouvelleCommandeViewModel(
        ICommandesApiClient commandesApi,
        IFournisseursApiClient fournisseursApi,
        IVinsApiClient vinsApi)
    {
        _commandesApi = commandesApi;
        _fournisseursApi = fournisseursApi;
        _vinsApi = vinsApi;

        FournisseursDisponibles = new ObservableCollection<FournisseurDto>();
        VinsDisponibles = new ObservableCollection<VinDto>();
        Panier = new ObservableCollection<LignePanierCommandeItem>();

        Panier.CollectionChanged += (s, e) =>
        {
            (EnregistrerBrouillonCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ValiderCommand as RelayCommand)?.RaiseCanExecuteChanged();
        };

        AjouterLigneCommand = new RelayNavCommand(AjouterLigne, PeutAjouterLigne);
        RetirerLigneCommand = new RelayNavCommand<LignePanierCommandeItem>(RetirerLigne);
        EnregistrerBrouillonCommand = new RelayCommand(
            () => EnregistrerAsync(validerImmediatement: false), PeutEnregistrer);
        ValiderCommand = new RelayCommand(
            () => EnregistrerAsync(validerImmediatement: true), PeutEnregistrer);
        AnnulerCommand = new RelayNavCommand(() => Annulee?.Invoke(this, EventArgs.Empty));

        _ = ChargerDonneesAsync();
    }

    public ObservableCollection<FournisseurDto> FournisseursDisponibles { get; }
    public ObservableCollection<VinDto> VinsDisponibles { get; }
    public ObservableCollection<LignePanierCommandeItem> Panier { get; }

    public bool EstEdition => _commandeExistante is not null;
    public string Titre => EstEdition
        ? $"Modifier brouillon n°{_commandeExistante!.Id}"
        : "Nouvelle commande";

    public FournisseurDto? FournisseurSelectionne
    {
        get => _fournisseurSelectionne;
        set
        {
            if (SetProperty(ref _fournisseurSelectionne, value))
            {
                (EnregistrerBrouillonCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ValiderCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public VinDto? VinAAjouter
    {
        get => _vinAAjouter;
        set
        {
            if (SetProperty(ref _vinAAjouter, value))
                (AjouterLigneCommand as RelayNavCommand)?.RaiseCanExecuteChanged();
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
            {
                (EnregistrerBrouillonCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ValiderCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand AjouterLigneCommand { get; }
    public ICommand RetirerLigneCommand { get; }
    public ICommand EnregistrerBrouillonCommand { get; }
    public ICommand ValiderCommand { get; }
    public ICommand AnnulerCommand { get; }

    public event EventHandler<CommandeDto>? CommandeEnregistree;
    public event EventHandler? Annulee;

    private async Task ChargerDonneesAsync()
    {
        try
        {
            var fournisseurs = await _fournisseursApi.GetTousAsync();
            FournisseursDisponibles.Clear();
            foreach (var f in fournisseurs.OrderBy(f => f.Nom)) FournisseursDisponibles.Add(f);

            var vins = await _vinsApi.GetTousAsync();
            VinsDisponibles.Clear();
            foreach (var v in vins.OrderBy(v => v.Nom)) VinsDisponibles.Add(v);
        }
        catch (Exception ex) { MessageErreur = $"Erreur chargement : {ex.Message}"; }
    }

    public async Task ChargerCommandeExistanteAsync(int commandeId)
    {
        try
        {
            EnregistrementEnCours = true;
            await ChargerDonneesAsync();

            _commandeExistante = await _commandesApi.GetParIdAsync(commandeId);
            if (_commandeExistante is null)
            {
                MessageErreur = $"Commande {commandeId} introuvable.";
                return;
            }

            FournisseurSelectionne = FournisseursDisponibles
                .FirstOrDefault(f => f.Id == _commandeExistante.FournisseurId);

            Panier.Clear();
            foreach (var ligne in _commandeExistante.Lignes)
            {
                var vin = VinsDisponibles.FirstOrDefault(v => v.Id == ligne.VinId);
                if (vin is not null)
                    Panier.Add(new LignePanierCommandeItem(vin, ligne.Quantite, ligne.Id));
            }

            OnPropertyChanged(nameof(Titre));
            OnPropertyChanged(nameof(EstEdition));
        }
        catch (Exception ex) { MessageErreur = ex.Message; }
        finally { EnregistrementEnCours = false; }
    }

    private bool PeutAjouterLigne()
        => VinAAjouter is not null && _quantiteAAjouter > 0;

    private void AjouterLigne()
    {
        if (VinAAjouter is null) return;

        // Si le vin est déjà au panier, on incrémente la quantité existante
        var existante = Panier.FirstOrDefault(l => l.Vin.Id == VinAAjouter.Id);
        if (existante is not null)
        {
            existante.Quantite += _quantiteAAjouter;
        }
        else
        {
            Panier.Add(new LignePanierCommandeItem(VinAAjouter, _quantiteAAjouter));
        }

        VinAAjouter = null;
        QuantiteAAjouter = 1;
        MessageErreur = string.Empty;
    }

    private void RetirerLigne(LignePanierCommandeItem? ligne)
    {
        if (ligne is not null) Panier.Remove(ligne);
    }

    private bool PeutEnregistrer()
        => FournisseurSelectionne is not null
        && Panier.Count > 0
        && !EnregistrementEnCours;

    private async Task EnregistrerAsync(bool validerImmediatement)
    {
        if (!PeutEnregistrer()) return;

        try
        {
            EnregistrementEnCours = true;
            MessageErreur = string.Empty;

            CommandeDto commande;

            // MODE EDITION 
            if (EstEdition)
            {
                var dto = new UpdateCommandeDto
                {
                    FournisseurId = FournisseurSelectionne!.Id,
                    Lignes = Panier.Select(l => new UpdateLigneCommandeDto
                    {
                        Id = l.IdLigneExistante,
                        VinId = l.Vin.Id,
                        Quantite = l.Quantite,
                        PrixUnitaire = 1m // placeholder: remplacer par la vraie logique de calcul du prix unitaire
                    }).ToList()
                };
                commande = await _commandesApi.ModifierAsync(_commandeExistante!.Id, dto);
            }
            else
            // MODE CREATION
            {
                var dto = new CreerCommandeDto
                {
                    FournisseurId = FournisseurSelectionne!.Id,
                    Lignes = Panier.Select(l => new CreerLigneCommandeDto
                    {
                        VinId = l.Vin.Id,
                        Quantite = l.Quantite,
                    }).ToList()
                };
                commande = await _commandesApi.CreerAsync(dto);
            }

            if (validerImmediatement)
                commande = await _commandesApi.ValiderAsync(commande.Id);

            CommandeEnregistree?.Invoke(this, commande);
        }
        catch (Exception ex) { MessageErreur = ex.Message; }
        finally { EnregistrementEnCours = false; }
    }
}



public class LignePanierCommandeItem : ViewModelBase
{
    private int _quantite;

    public LignePanierCommandeItem(VinDto vin, int quantite, int? idExistant = null)
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
        set => SetProperty(ref _quantite, value);
    }

}