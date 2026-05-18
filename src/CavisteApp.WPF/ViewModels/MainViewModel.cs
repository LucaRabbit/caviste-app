// ViewModels/MainViewModel.cs
using System.Windows.Input;
using CavisteApp.WPF.Services;
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

        // Pour une future vue :
        // AfficherClientsCommand = new RelayNavCommand(
        //     () => CurrentViewModel = _services.GetRequiredService<ClientsViewModel>());
    }

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public string MessageBienvenue =>
        _session.Utilisateur is null
            ? string.Empty
            : $"Bienvenue {_session.Utilisateur.Login} ({_session.Utilisateur.Role}) 👤";

    public ICommand AfficherVinsCommand { get; }
    public ICommand AfficherFournisseursCommand
    {
        get;
    }
}