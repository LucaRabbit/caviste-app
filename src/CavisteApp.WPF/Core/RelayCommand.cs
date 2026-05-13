using System.Windows.Input;
namespace Projet_Commun.Core
{
    // ICommand
    // interface du framework .NET Permet de lier une action à un élément de l'interface utilisateur
    // (comme un bouton) via le data binding.
    internal class RelayCommand : ICommand
    {
        // Action<object> :
        // Délégué qui représente une méthode qui prend un paramètre de type object et ne retourne rien.
        private Action<object> _execute;
       private Func<object, bool> _canExecute;
        // EventHandler :
        // Délégué qui représente une méthode qui gère un événement,
        // Prenant un objet sender et des EventArgs.
        public event EventHandler CanExecuteChanged
        {
            // CommandManager.RequerySuggested :
            // Événement statique du CommandManager qui est déclenché lorsque le système
            // Estime que les commandes doivent être réévaluées.
            add { CommandManager.RequerySuggested += value;}
            remove { CommandManager.RequerySuggested -= value;}
        }
        
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }
        public bool CanExecute(object parameter)
        {
            //|| : Opérateur logique OU
            return _canExecute == null || _canExecute(parameter);
        }
        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
