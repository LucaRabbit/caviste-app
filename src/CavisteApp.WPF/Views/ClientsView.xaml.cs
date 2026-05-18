using System.Windows.Controls;
using CavisteApp.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CavisteApp.WPF.Views
{
    /// <summary>
    /// Logique d'interaction pour ClientsView.xaml
    /// </summary>
    public partial class ClientsView : UserControl
    {
        public ClientsView()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<ClientsViewModel>();

        }
    }
}
