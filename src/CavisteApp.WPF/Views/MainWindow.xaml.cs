using CavisteApp.WPF.Services;
using CavisteApp.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace CavisteApp.WPF.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly SessionService _session;

    public MainWindow(SessionService session)
    {
        InitializeComponent();
        _session = session;
        DataContext = new MainViewModel(session);


    }

    private void BtnDeconnexion_Click(object sender, RoutedEventArgs e)
    {
        _session.Deconnecter();

        // Réouvrir la fenêtre de login
        var loginWindow = App.Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();
        Close();
    }
}