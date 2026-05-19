// ViewModels/ImportVinsViewModel.cs
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Http;
using System.Windows.Input;
using CavisteApp.DTOs.Import;
using CavisteApp.WPF.Services.ApiClient;

namespace CavisteApp.WPF.ViewModels;

public class ImportVinsViewModel : ViewModelBase
{
    private readonly IImportApiClient _importApi;

    private string _recherche = string.Empty;
    private string _limiteTexte = "20";
    private int _limite = 20;
    private bool _enChargement;
    private bool _enImport;
    private string _messageErreur = string.Empty;
    private string _messageSuccess = string.Empty;

    public ImportVinsViewModel(IImportApiClient importApi)
    {
        _importApi = importApi;
        Resultats = new ObservableCollection<WineSearchResultDto>();

        RechercherCommand = new RelayCommand(RechercherAsync, PeutRechercher);
        ImporterCommand = new RelayCommand(ImporterAsync, PeutImporter);
    }

    public ObservableCollection<WineSearchResultDto> Resultats { get; }

    public string Recherche
    {
        get => _recherche;
        set
        {
            if (SetProperty(ref _recherche, value))
                (RechercherCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public string LimiteTexte
    {
        get => _limiteTexte;
        set
        {
            if (SetProperty(ref _limiteTexte, value))
            {
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out var v) && v > 0)
                    _limite = v;
                else
                    _limite = 0;
                (RechercherCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public bool EnChargement
    {
        get => _enChargement;
        set
        {
            if (SetProperty(ref _enChargement, value))
                (RechercherCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public bool EnImport
    {
        get => _enImport;
        set
        {
            if (SetProperty(ref _enImport, value))
                (ImporterCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    public string MessageErreur
    {
        get => _messageErreur;
        set => SetProperty(ref _messageErreur, value);
    }

    public string MessageSuccess
    {
        get => _messageSuccess;
        set => SetProperty(ref _messageSuccess, value);
    }

    public ICommand RechercherCommand { get; }
    public ICommand ImporterCommand { get; }

    private bool PeutRechercher() => !string.IsNullOrWhiteSpace(_recherche) && _limite > 0 && !EnChargement;
    private bool PeutImporter() => Resultats.Count > 0 && !EnImport;

    private async Task RechercherAsync()
    {
        try
        {
            EnChargement = true;
            MessageErreur = string.Empty;
            MessageSuccess = string.Empty;
            Resultats.Clear();

            var resultats = await _importApi.RechercherAsync(_recherche, _limite);
            foreach (var r in resultats) Resultats.Add(r);

            if (Resultats.Count == 0)
                MessageErreur = "Aucun vin trouvé pour cette recherche.";

            (ImporterCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
        catch (Exception ex) { MessageErreur = ex.Message; }
        finally { EnChargement = false; }
    }

    private async Task ImporterAsync()
    {
        try
        {
            EnImport = true;
            MessageErreur = string.Empty;
            MessageSuccess = string.Empty;

            var resultat = await _importApi.ImporterAsync(_recherche, _limite);

            MessageSuccess = resultat.Message;

            // Optionnel : vider la liste après import réussi
            Resultats.Clear();
            (ImporterCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
        catch (Exception ex) { MessageErreur = ex.Message; }
        finally { EnImport = false; }
    }
}