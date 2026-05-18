using System.Windows;
using CavisteApp.WPF.ViewModels.Editing;

namespace CavisteApp.WPF.Views;

public partial class EditWindow : Window
{
    private readonly IEditViewModel _vm;

    public EditWindow(IEditViewModel viewModel)
    {
        InitializeComponent();
        _vm = viewModel;
        DataContext = viewModel;
    }

    private async void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
    {
        if (await _vm.EnregistrerAsync())
        {
            DialogResult = true;
            Close();
            // Ferme la fenêtre et indique que l'enregistrement a réussi
        }
    }

    private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}