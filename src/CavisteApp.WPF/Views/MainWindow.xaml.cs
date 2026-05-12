using System.Windows;
using CavisteApp.WPF.Services;
using Microsoft.Extensions.DependencyInjection;

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

        if (_session.Utilisateur is not null)
        {
            LblBienvenue.Text = $"Bienvenue {_session.Utilisateur.Login} ({_session.Utilisateur.Role})";
        }
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