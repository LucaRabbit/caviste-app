using System.Windows;
using System.Windows.Controls;
using CavisteApp.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Logique d'interaction pour LoginWindow.xaml
    /// </summary>
namespace CavisteApp.WPF.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.LoginReussi += OnLoginReussi;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox pb)
        {
            _viewModel.Password = pb.Password;
        }
    }

    private void OnLoginReussi(object? sender, EventArgs e)
    {
        var mainWindow = App.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
        Close();
    }
}

