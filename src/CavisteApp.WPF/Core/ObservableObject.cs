using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Projet_Commun.Core
// INotifyPropertyChanged
// interface du framework .NET Permet de notifier l'interface graphique qu'une de ses propriétés a changé
// L'UI se mette à jour automatiquement via le data binding.
{
    internal class ObservableObject : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;
        // [CallerMemberName]
        // Permet de récupérer le nom de la propriété qui a appelé la méthode, évitant ainsi d'avoir à le spécifier manuellement.
        protected void OnPropertyChanged([CallerMemberName]string Name=null)
        {
            // ?.Invoke : Permet d'invoquer l'événement de manière sécurisée,
            // En vérifiant d'abord s'il y a des abonnés à l'événement avant de l'invoquer.
            // new PropertyChangedEventArgs(Name) :
            // Crée une nouvelle instance de PropertyChangedEventArgs avec le nom de la propriété qui a changé.

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
        }
    }
}
