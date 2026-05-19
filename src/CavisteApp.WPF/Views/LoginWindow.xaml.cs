using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        App.MainAppWindow = mainWindow;
        mainWindow.Show();
        Close();
    }

    private void BtnFermer_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }

}

