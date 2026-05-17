using System.Windows;
using CavisteApp.WPF.Services;
using CavisteApp.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CavisteApp.WPF.Views;

public partial class MainWindow : Window
{
    private readonly SessionService _session;

    public MainWindow(MainViewModel viewModel, SessionService session)
    {
        InitializeComponent();
        DataContext = viewModel;
        _session = session;
    }

    private void BtnDeconnexion_Click(object sender, RoutedEventArgs e)
    {
        _session.Deconnecter();
        var loginWindow = App.Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();
        Close();
    }
}