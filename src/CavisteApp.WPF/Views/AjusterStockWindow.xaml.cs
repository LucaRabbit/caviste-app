using System.Windows;
using CavisteApp.WPF.ViewModels;

namespace CavisteApp.WPF.Views;

public partial class AjusterStockWindow : Window
{
    public AjusterStockWindow(AjusterStockViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.Validee += (s, e) => { DialogResult = true; Close(); };
        viewModel.Annule += (s, e) => { DialogResult = false; Close(); };
    }
}