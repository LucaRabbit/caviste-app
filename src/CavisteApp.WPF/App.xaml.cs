using System.IO;
using System.Windows;
using CavisteApp.WPF.Services;
using CavisteApp.WPF.Services.ApiClient;
using CavisteApp.WPF.ViewModels;
using CavisteApp.WPF.Views;
using CavisteApp.WPF.Views.Editing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CavisteApp.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static IConfiguration Configuration { get; private set; } = null!;
    public static Window? MainAppWindow { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);


        // Handler global pour voir les exceptions
        DispatcherUnhandledException += (s, args) =>
        {
            MessageBox.Show(
                $"Erreur non gérée :\n\n{args.Exception}",
                "Crash",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;   // empêche le crash
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            if (args.ExceptionObject is Exception ex)
                MessageBox.Show($"Exception terminale :\n\n{ex}", "Crash fatal");
        };

        TaskScheduler.UnobservedTaskException += (s, args) =>
        {
            MessageBox.Show($"Exception async non observée :\n\n{args.Exception}", "Crash async");
            args.SetObserved();
        };


        // Configuration
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        Configuration = builder;

        // Ajouter les variables d'environnement
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Afficher la fenêtre de login
        var loginWindow = Services.GetRequiredService<LoginWindow>();
        loginWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IConfiguration>(Configuration);

        // Services (singleton pour partager la session utilisateur)
        services.AddSingleton<SessionService>();
        services.AddTransient<AuthHttpHandler>();

        var apiBaseUrl = Configuration["Api:BaseUrl"] ?? throw new InvalidOperationException("Configuration Api:BaseUrl manquante dans appsettings.");

        // Client authentifié pour les appels API
        services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
        });

        // Client pour les opérations sur les vins, avec injection automatique du token JWT via AuthHttpHandler
        services.AddHttpClient<IVinsApiClient, VinsApiClient>(c =>
        {
            c.BaseAddress = new Uri(apiBaseUrl);
        })
        .AddHttpMessageHandler<AuthHttpHandler>();

        // Client pour les opérations sur les fournisseurs, avec injection automatique du token JWT via AuthHttpHandler
        services.AddHttpClient<IFournisseursApiClient, FournisseursApiClient>(c =>
        {
            c.BaseAddress = new Uri(apiBaseUrl);
        }) .AddHttpMessageHandler<AuthHttpHandler>();

        // Client pour les opérations sur les clients, avec injection automatique du token JWT via AuthHttpHandler
        services.AddHttpClient<IClientsApiClient, ClientsApiClient>(c =>
        {
            c.BaseAddress = new Uri(apiBaseUrl);
        })
        .AddHttpMessageHandler<AuthHttpHandler>();

        // Client pour les opérations sur les ventes, avec injection automatique du token JWT via AuthHttpHandler
        services.AddHttpClient<IVentesApiClient, VentesApiClient>(c =>
        {
            c.BaseAddress = new Uri(apiBaseUrl);
        })
        .AddHttpMessageHandler<AuthHttpHandler>();

        // Client pour les opérations sur les commandes, avec injection automatique du token JWT via AuthHttpHandler
        services.AddHttpClient<ICommandesApiClient, CommandesApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
        .AddHttpMessageHandler<AuthHttpHandler>();

        // CLient pour les opérations d'import, avec injection automatique du token JWT via AuthHttpHandler
        services.AddHttpClient<IImportApiClient, ImportApiClient>(c => c.BaseAddress = new Uri(apiBaseUrl))
                .AddHttpMessageHandler<AuthHttpHandler>();

        // ViewModels (transient pour créer une nouvelle instance à chaque fois)
        services.AddTransient<LoginViewModel>();
        services.AddTransient<VinsViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<FournisseursViewModel>();
        services.AddTransient<ClientsViewModel>();
        services.AddTransient<NouvelleVenteViewModel>();
        services.AddTransient<NouvelleCommandeViewModel>();
        services.AddTransient<ImportVinsViewModel>();

        // Vues (transient pour créer une nouvelle instance à chaque fois)
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();
        services.AddTransient<VinsView>();
        services.AddTransient<EditWindow>();
        services.AddTransient<VinEditView>();
        services.AddTransient<FournisseurEditView>();
        services.AddTransient<FournisseursView>();
        services.AddTransient<ClientsView>();
        services.AddTransient<ClientEditView>();
        services.AddTransient<VentesView>();
        services.AddTransient<NouvelleVenteView>();
        services.AddTransient<CommandesView>();
        services.AddTransient<NouvelleCommandeView>();
        services.AddTransient<ImportVinsView>();

    }
}
