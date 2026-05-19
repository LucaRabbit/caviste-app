using System.Globalization;
using System.Net.Http;
using System.Windows.Input;
using CavisteApp.DTOs.Enums;
using CavisteApp.DTOs.Vins;
using CavisteApp.WPF.Services.ApiClient;

namespace CavisteApp.WPF.ViewModels;

public enum ModeAjustement
{
    Inventaire,
    Retrait
}

public class AjusterStockViewModel : ViewModelBase
{
    private readonly IVinsApiClient _vinsApi;
    private readonly VinDto _vin;

    private string _stockReelTexte;
    private int _stockReel;

    private string _quantiteRetraitTexte = "1";
    private int _quantiteRetrait = 1;

    private MotifRetraitStock _motif = MotifRetraitStock.Casse;
    private string? _commentaire;
    private string _messageErreur = string.Empty;
    private bool _enCours;

    public AjusterStockViewModel(IVinsApiClient vinsApi, VinDto vin, ModeAjustement mode)
    {
        _vinsApi = vinsApi;
        _vin = vin;
        Mode = mode;

        _stockReelTexte = vin.Stock.ToString();
        _stockReel = vin.Stock;

        MotifsDisponibles = Enum.GetValues<MotifRetraitStock>();

        EnregistrerCommand = new RelayCommand(EnregistrerAsync, PeutEnregistrer);
        AnnulerCommand = new RelayNavCommand(() => Annule?.Invoke(this, EventArgs.Empty));
    }

    public ModeAjustement Mode { get; }
    public bool EstInventaire => Mode == ModeAjustement.Inventaire;
    public bool EstRetrait => Mode == ModeAjustement.Retrait;

    public string Titre => EstInventaire
        ? $"Inventaire : {_vin.Nom}"
        : $"Retrait de stock : {_vin.Nom}";

    public int StockActuel => _vin.Stock;
    public MotifRetraitStock[] MotifsDisponibles { get; }

    // ─── Champs Inventaire ──────────────────────────────────
    public string StockReelTexte
    {
        get => _stockReelTexte;
        set
        {
            if (SetProperty(ref _stockReelTexte, value))
            {
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out var v) && v >= 0)
                    _stockReel = v;
                else
                    _stockReel = -1;   // marqueur d'invalidité
                (EnregistrerCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    // ─── Champs Retrait ─────────────────────────────────────
    public string QuantiteRetraitTexte
    {
        get => _quantiteRetraitTexte;
        set
        {
            if (SetProperty(ref _quantiteRetraitTexte, value))
            {
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out var v) && v > 0)
                    _quantiteRetrait = v;
                else
                    _quantiteRetrait = 0;
                (EnregistrerCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public MotifRetraitStock Motif
    {
        get => _motif;
        set => SetProperty(ref _motif, value);
    }

    // ─── Commun ─────────────────────────────────────────────
    public string? Commentaire
    {
        get => _commentaire;
        set => SetProperty(ref _commentaire, value);
    }

    public string MessageErreur
    {
        get => _messageErreur;
        set => SetProperty(ref _messageErreur, value);
    }

    public bool EnCours
    {
        get => _enCours;
        set
        {
            if (SetProperty(ref _enCours, value))
                (EnregistrerCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public ICommand EnregistrerCommand { get; }
    public ICommand AnnulerCommand { get; }

    public event EventHandler? Validee;
    public event EventHandler? Annule;

    private bool PeutEnregistrer()
    {
        if (EnCours) return false;
        return EstInventaire
            ? _stockReel >= 0
            : _quantiteRetrait > 0 && _quantiteRetrait <= _vin.Stock;
    }

    private async Task EnregistrerAsync()
    {
        if (!PeutEnregistrer()) return;

        try
        {
            EnCours = true;
            MessageErreur = string.Empty;

            if (EstInventaire)
            {
                await _vinsApi.AjusterStockAsync(_vin.Id, new InventaireDto
                {
                    StockReel = _stockReel,
                    Commentaire = Commentaire
                });
            }
            else
            {
                await _vinsApi.RetirerStockAsync(_vin.Id, new RetraitStockDto
                {
                    Quantite = _quantiteRetrait,
                    Motif = (int)Motif,
                    Commentaire = Commentaire
                });
            }

            Validee?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex) { MessageErreur = ex.Message; }
        finally { EnCours = false; }
    }
}