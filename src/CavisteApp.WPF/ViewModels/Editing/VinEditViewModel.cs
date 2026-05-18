// ViewModels/Editing/VinEditViewModel.cs
using System.Net.Http;
using CavisteApp.DTOs.Enums;
using CavisteApp.DTOs.Vins;
using CavisteApp.WPF.Services.ApiClient;
using System.Globalization;

namespace CavisteApp.WPF.ViewModels.Editing;

public class VinEditViewModel : EditViewModelBase
{
    private readonly IVinsApiClient _api;
    private readonly VinDto? _vinExistant;   // null = création

    public VinEditViewModel(IVinsApiClient api, VinDto? vinExistant = null)
    {
        _api = api;
        _vinExistant = vinExistant;
        TypesVin = Enum.GetValues<TypeVin>();

        if (vinExistant is null)
        {
            Type = TypesVin.First();
            // Force la validation initiale en passant par les setters
            PrixTexte = "0";
            StockActuelTexte = "0";
            SeuilStockBasTexte = "0";
        }
        else
        {
            Nom = vinExistant.Nom;
            Type = vinExistant.Type;
            PrixTexte = vinExistant.Prix.ToString(CultureInfo.CurrentCulture);
            StockActuelTexte = vinExistant.Stock.ToString();
            SeuilStockBasTexte = vinExistant.SeuilStockBas.ToString();
        }

        DefinirErreur(string.IsNullOrWhiteSpace(Nom)? "* Le nom est obligatoire." : null, nameof(Nom));
    }

    public override string Titre => _vinExistant is null
        ? "Nouveau vin"
        : $"Modifier : {_vinExistant.Nom}";

    public bool EstModification => _vinExistant is not null;

    private string _nom = string.Empty;
    public string Nom
    {
        get => _nom;
        set
        {
            if (SetProperty(ref _nom, value))
            {
                DefinirErreur(string.IsNullOrWhiteSpace(value)
                    ? "* Le nom est obligatoire"
                    : null);
            }
        }
    }

    // ─── Prix ───────────────────────────────────────────────
    private decimal _prix;
    private string _prixTexte = "0";
    public string PrixTexte
    {
        get => _prixTexte;
        set
        {
            if (SetProperty(ref _prixTexte, value))
                ValiderPrix(value);
        }
    }

    private void ValiderPrix(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            DefinirErreur("* Ce champ est obligatoire.", nameof(PrixTexte));
            return;
        }

        // Normaliser : remplacer la virgule par un point, puis parser en culture invariante
        var normalise = s.Trim().Replace(',', '.');

        if (!decimal.TryParse(normalise, NumberStyles.Number, CultureInfo.InvariantCulture, out var v))
            DefinirErreur("Veuillez saisir un nombre valide (ex: 12.50 ou 12,50).", nameof(PrixTexte));
        else if (v < 0)
            DefinirErreur("Le prix ne peut pas être négatif.", nameof(PrixTexte));
        else
        {
            _prix = v;
            DefinirErreur(null, nameof(PrixTexte));
        }
    }

    // ─── Stock actuel ───────────────────────────────────────
    private int _stockActuel;
    private string _stockActuelTexte = "0";
    public string StockActuelTexte
    {
        get => _stockActuelTexte;
        set
        {
            if (SetProperty(ref _stockActuelTexte, value))
                ValiderStock(value);
        }
    }

    private void ValiderStock(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            DefinirErreur("* Ce champ est obligatoire.", nameof(StockActuelTexte));
        else if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.CurrentCulture, out var v))
            DefinirErreur("Veuillez saisir un nombre entier.", nameof(StockActuelTexte));
        else if (v < 0)
            DefinirErreur("Le stock ne peut pas être négatif.", nameof(StockActuelTexte));
        else
        {
            _stockActuel = v;
            DefinirErreur(null, nameof(StockActuelTexte));
        }
    }

    // ─── Seuil stock bas ────────────────────────────────────
    private int _seuilStockBas;
    private string _seuilStockBasTexte = "0";
    public string SeuilStockBasTexte
    {
        get => _seuilStockBasTexte;
        set
        {
            if (SetProperty(ref _seuilStockBasTexte, value))
                ValiderSeuil(value);
        }
    }

    private void ValiderSeuil(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            DefinirErreur("* Ce champ est obligatoire.", nameof(SeuilStockBasTexte));
        else if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.CurrentCulture, out var v))
            DefinirErreur("Veuillez saisir un nombre entier.", nameof(SeuilStockBasTexte));
        else if (v < 0)
            DefinirErreur("Le seuil ne peut pas être négatif.", nameof(SeuilStockBasTexte));
        else
        {
            _seuilStockBas = v;
            DefinirErreur(null, nameof(SeuilStockBasTexte));
        }
    }

    public TypeVin[] TypesVin { get; }

    private TypeVin _type;
    public TypeVin Type { get => _type; set => SetProperty(ref _type, value); }


    /// Le vin créé ou modifié (rempli après Enregistrer réussi)
    public VinDto? Resultat { get; private set; }

    public override async Task<bool> EnregistrerAsync()
    {
        if (!EstValide)
        {
            MessageErreur = "Veuillez corriger les erreurs.";
            return false;
        }

        try
        {
            EnregistrementEnCours = true;
            MessageErreur = string.Empty;

            if (_vinExistant is null)
            {
                Resultat = await _api.CreerAsync(new CreerVinDto
                {
                    Nom = Nom,
                    Type = Type,
                    Prix = _prix,
                    SeuilStockBas = _seuilStockBas
                });
            }
            else
            {
                await _api.ModifierAsync(_vinExistant.Id, new UpdateVinDto
                {
                    Nom = Nom,
                    Type = Type,
                    Prix = _prix,
                    SeuilStockBas = _seuilStockBas
                });

                Resultat = await _api.GetParIdAsync(_vinExistant.Id); ;
            }
            return true;
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("403"))
        {
            MessageErreur = "Action réservée à l'administrateur.";
            return false;
        }
        catch (Exception ex)
        {
            MessageErreur = ex.Message;
            return false;
        }
        finally
        {
            EnregistrementEnCours = false;
        }
    }
}