using System.Windows;

namespace CavisteApp.WPF.Services.ApiClient;

public static class ApiErrorHelper
{
    /// <summary>
    /// Extrait le message lisible d'une exception API.
    /// Format attendu : "Erreur API (XXX) : message"
    /// </summary>
    public static string ExtraireMessage(Exception ex)
    {
        var brut = ex.Message;
        var index = brut.IndexOf(" : ");
        return index > 0 ? brut[(index + 3)..] : brut;
    }

    /// <summary>
    /// Affiche un message d'erreur API dans une MessageBox.
    /// </summary>
    public static void AfficherErreur(Exception ex, string titre = "Erreur")
    {
        var message = ExtraireMessage(ex);
        MessageBox.Show(message, titre, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}