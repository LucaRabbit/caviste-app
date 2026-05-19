using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

using CavisteApp.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CavisteApp.WPF.Views
{
    /// <summary>
    /// Logique d'interaction pour FournisseursView.xaml
    /// </summary>
    public partial class FournisseursView : UserControl
    {
        public FournisseursView()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<FournisseursViewModel>();
        }
    }
}