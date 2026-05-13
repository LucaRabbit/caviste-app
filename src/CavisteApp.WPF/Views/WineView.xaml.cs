using CavisteApp.WPF.Services;
using CavisteApp.WPF.ViewModels;
using System.Windows.Controls;

namespace CavisteApp.WPF.Views
{
    public partial class WineView : Page
    {
        public WineView(SessionService sessionService)
        {
            InitializeComponent();
            DataContext = new MainViewModel(sessionService);
        }
    }
}