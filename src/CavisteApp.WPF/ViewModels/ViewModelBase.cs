using CavisteApp.WPF.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CavisteApp.WPF.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        // Implémentation de INotifyPropertyChanged pour la liaison de données
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
    
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }


    }
    public class MainViewModel : ViewModelBase
    {
        private readonly SessionService _sessionService;

        public bool IsAdmin => _sessionService.IsAdmin;

        public MainViewModel(SessionService sessionService)
        {
            _sessionService = sessionService;

            // SessionChanged existe déjà dans ton service, on s'y abonne
            _sessionService.SessionChanged += (s, e) => OnPropertyChanged(nameof(IsAdmin));
        }
    }
}
