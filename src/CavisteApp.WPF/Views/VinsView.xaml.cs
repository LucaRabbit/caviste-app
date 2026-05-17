using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

using CavisteApp.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CavisteApp.WPF.Views
{
    /// <summary>
    /// Logique d'interaction pour VinsView.xaml
    /// </summary>
    public partial class VinsView : UserControl
    {
        public VinsView()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<VinsViewModel>();
        }
    }
}
