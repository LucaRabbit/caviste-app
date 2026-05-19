using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using CavisteApp.WPF.Services;
using CavisteApp.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CavisteApp.WPF.Views;

public partial class MainWindow : Window
{
    private readonly SessionService _session;
    public string IconeMaximiser =>
        WindowState == WindowState.Maximized ? "❐" : "⬜";

    public MainWindow(MainViewModel viewModel, SessionService session)
    {
        InitializeComponent();
        DataContext = viewModel;
        _session = session;

        StateChanged += (s, e) => OnPropertyChanged(nameof(IconeMaximiser));
    }

    private void BtnDeconnexion_Click(object sender, RoutedEventArgs e)
    {
        _session.Deconnecter();
        var loginWindow = App.Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();
        Close();
    }

    private void BtnFermer_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void BtnMinimiser_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void BtnMaximiser_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && WindowState == WindowState.Normal)
            DragMove();
    }

    // INotifyPropertyChanged pour que le binding sur IconeMaximiser se mette à jour
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Code-behind
    private void Container_MouseEnter(object sender, MouseEventArgs e)
    {
        var sb = new Storyboard();

        // Cercle 1 → se déplace à droite et grossit
        var moveC1 = new DoubleAnimation(190, TimeSpan.FromSeconds(0.3));
        Storyboard.SetTargetName(moveC1, "Circle1");
        Storyboard.SetTargetProperty(moveC1, new PropertyPath("Margin.Left"));

        var scaleC1X = new DoubleAnimation(1.2, TimeSpan.FromSeconds(0.3));
        Storyboard.SetTargetName(scaleC1X, "Circle1Scale");
        Storyboard.SetTargetProperty(scaleC1X, new PropertyPath("ScaleX"));

        var scaleC1Y = new DoubleAnimation(1.2, TimeSpan.FromSeconds(0.3));
        Storyboard.SetTargetName(scaleC1Y, "Circle1Scale");
        Storyboard.SetTargetProperty(scaleC1Y, new PropertyPath("ScaleY"));

        // Cercle 2 → se déplace à gauche et grossit
        var moveC2 = new DoubleAnimation(-10, TimeSpan.FromSeconds(0.3));
        Storyboard.SetTargetName(moveC2, "Circle2");
        Storyboard.SetTargetProperty(moveC2, new PropertyPath("Margin.Left"));

        var scaleC2X = new DoubleAnimation(1.2, TimeSpan.FromSeconds(0.3));
        Storyboard.SetTargetName(scaleC2X, "Circle2Scale");
        Storyboard.SetTargetProperty(scaleC2X, new PropertyPath("ScaleX"));

        var scaleC2Y = new DoubleAnimation(1.2, TimeSpan.FromSeconds(0.3));
        Storyboard.SetTargetName(scaleC2Y, "Circle2Scale");
        Storyboard.SetTargetProperty(scaleC2Y, new PropertyPath("ScaleY"));

        sb.Children.Add(moveC1);
        sb.Children.Add(scaleC1X);
        sb.Children.Add(scaleC1Y);
        sb.Children.Add(moveC2);
        sb.Children.Add(scaleC2X);
        sb.Children.Add(scaleC2Y);

        sb.Begin((Border)sender);
    }

    private void Container_MouseLeave(object sender, MouseEventArgs e)
    {
        var sb = new Storyboard();

        var moveC1 = new DoubleAnimation(-20, TimeSpan.FromSeconds(0.3));
        Storyboard.SetTargetName(moveC1, "Circle1");
        Storyboard.SetTargetProperty(moveC1, new PropertyPath("Margin.Left"));

        var scaleC1X = new DoubleAnimation(1.0, TimeSpan.FromSeconds(0.3));
        Storyboard.SetTargetName(scaleC1X, "Circle1Scale");
        Storyboard.SetTargetProperty(scaleC1X, new PropertyPath("ScaleX"));

        var scaleC1Y = new DoubleAnimation(1.0, TimeSpan.FromSeconds(0.3));
        Storyboard.SetTargetName(scaleC1Y, "Circle1Scale");
        Storyboard.SetTargetProperty(scaleC1Y, new PropertyPath("ScaleY"));

        var moveC2 = new DoubleAnimation(183, TimeSpan.FromSeconds(0.3));
        Storyboard.SetTargetName(moveC2, "Circle2");
        Storyboard.SetTargetProperty(moveC2, new PropertyPath("Margin.Left"));

        var scaleC2X = new DoubleAnimation(1.0, TimeSpan.FromSeconds(0.3));
        Storyboard.SetTargetName(scaleC2X, "Circle2Scale");
        Storyboard.SetTargetProperty(scaleC2X, new PropertyPath("ScaleX"));

        var scaleC2Y = new DoubleAnimation(1.0, TimeSpan.FromSeconds(0.3));
        Storyboard.SetTargetName(scaleC2Y, "Circle2Scale");
        Storyboard.SetTargetProperty(scaleC2Y, new PropertyPath("ScaleY"));

        sb.Children.Add(moveC1);
        sb.Children.Add(scaleC1X);
        sb.Children.Add(scaleC1Y);
        sb.Children.Add(moveC2);
        sb.Children.Add(scaleC2X);
        sb.Children.Add(scaleC2Y);

        sb.Begin((Border)sender);
    }
}