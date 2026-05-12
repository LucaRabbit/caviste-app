using System.Net.Http;
using System.Windows.Input;
using CavisteApp.DTOs.Auth;
using CavisteApp.WPF.Services;
using CavisteApp.WPF.Services.ApiClient;

namespace CavisteApp.WPF.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthApiClient _authApiClient;
    private readonly SessionService _sessionService;

    private string _login = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;
    private bool _isLoading;

    public LoginViewModel(IAuthApiClient authApiClient, SessionService sessionService)
    {
        _authApiClient = authApiClient;
        _sessionService = sessionService;
        LoginCommand = new RelayCommand(LoginAsync, () => !IsLoading);
    }

    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand LoginCommand { get;}

    public event EventHandler? LoginReussi;

    private async Task LoginAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var reponse = await _authApiClient.LoginAsync(new LoginRequest
            {
                Login = Login,
                Password = Password
            });

            if (reponse is null)
            {
                ErrorMessage = "Login ou mot de passe incorrect.";
                return;
            }

            _sessionService.Connecter(reponse);
            LoginReussi?.Invoke(this, EventArgs.Empty);
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Erreur réseau : impossible de joindre l'API. ({ex.Message})";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erreur : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
