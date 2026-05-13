using CavisteApp.WPF;
using CavisteApp.WPF.Services;
using CavisteApp.WPF.ViewModels;
using CavisteApp.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading.Tasks;
using CavisteApp.WPF.Core;

public class MainViewModel : ViewModelBase
{
    private readonly SessionService _session;
    private object _currentView = new TextBlock { Text = "Accueil" };
    private string _selectedMenu = "Accueil";

    public bool IsAdmin => _session.IsAdmin;

    public object CurrentView
    {
        get => _currentView;
        set
        {
            if (_currentView != value)
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }
    }

    public string SelectedMenu
    {
        get => _selectedMenu;
        set
        {
            if (_selectedMenu != value)
            {
                _selectedMenu = value;
                OnPropertyChanged(nameof(SelectedMenu));
                NaviguerVers(value);
            }
        }
    }

    public ICommand NaviguerVersHome { get; }
    public ICommand NaviguerVersVin { get; }

    public MainViewModel(SessionService session)
    {
        _session = session;
        _session.SessionChanged += (s, e) => OnPropertyChanged(nameof(IsAdmin));

        NaviguerVersHome = new RelayCommand(() =>
        {
            NaviguerVers("Accueil");
            return Task.CompletedTask;
        });

        NaviguerVersVin = new RelayCommand(() =>
        {
            NaviguerVers("Vin");
            return Task.CompletedTask;
        });
    }

    private void NaviguerVers(string page)
    {
        CurrentView = page switch
        {
            "Vin" => App.Services.GetRequiredService<WineView>(),
            "Stock" => new TextBlock { Text = "Page Stock" }, // À remplacer par StockView
            "Fourniseur" => new TextBlock { Text = "Page Fournisseur" }, // À remplacer par SupplierView
            "Panier" => new TextBlock { Text = "Page Panier" }, // À remplacer par CartView
            _ => new TextBlock { Text = "Accueil" } // Page d'accueil
        };
    }
}