// ViewModels/MainViewModel.cs
using System.Windows.Input;
using CavisteApp.WPF.Services;
using CavisteApp.WPF.Services.ApiClient;
using Microsoft.Extensions.DependencyInjection;

namespace CavisteApp.WPF.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly SessionService _session;
    private readonly IServiceProvider _services;

    private ViewModelBase? _currentViewModel;

    public MainViewModel(SessionService session, IServiceProvider services)
    {
        _session = session;
        _services = services;

        AfficherVinsCommand = new RelayNavCommand(
            () => CurrentViewModel = _services.GetRequiredService<VinsViewModel>());
        AfficherFournisseursCommand = new RelayNavCommand(
            () => CurrentViewModel = _services.GetRequiredService<FournisseursViewModel>());
        AfficherClientsCommand = new RelayNavCommand(
            () => CurrentViewModel = _services.GetRequiredService<ClientsViewModel>());
        AfficherVentesCommand = new RelayNavCommand(
            () => CurrentViewModel = CreerVentesViewModel());
        AfficherCommandesCommand = new RelayNavCommand(() => CurrentViewModel = CreerCommandesViewModel());
    }

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    private VentesViewModel CreerVentesViewModel()
    {
        return new VentesViewModel(
            _services.GetRequiredService<IVentesApiClient>(),
            _services.GetRequiredService<SessionService>(),
            () => _services.GetRequiredService<NouvelleVenteViewModel>(),
            vm => CurrentViewModel = vm);
    }

    private CommandesViewModel CreerCommandesViewModel()
    {
        return new CommandesViewModel(
            _services.GetRequiredService<ICommandesApiClient>(),
            _services.GetRequiredService<SessionService>(),
            () => _services.GetRequiredService<NouvelleCommandeViewModel>(),
            vm => CurrentViewModel = vm);
    }

    public string MessageBienvenue =>
        _session.Utilisateur is null
            ? string.Empty
            : $"Bienvenue {_session.Utilisateur.Login} ({_session.Utilisateur.Role}) 👤";

    public ICommand AfficherVinsCommand { get; }
    public ICommand AfficherFournisseursCommand { get; }
    public ICommand AfficherClientsCommand { get; }
    public ICommand AfficherVentesCommand { get; }
    public ICommand AfficherCommandesCommand { get; }
}