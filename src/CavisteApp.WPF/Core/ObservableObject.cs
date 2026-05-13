using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace CavisteApp.WPF.Core;

/// <summary>
/// Classe de base pour les objets observables qui implémentent INotifyPropertyChanged.
/// Permet de notifier l'interface graphique qu'une propriété a changé, 
/// permettant ainsi à l'UI de se mettre à jour automatiquement via le data binding.
/// </summary>
internal class ObservableObject : INotifyPropertyChanged
{
    /// <summary>
    /// Événement déclenché lorsqu'une propriété change.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Notifie les abonnés que la propriété spécifiée a changé.
    /// </summary>
    /// <param name="name">
    /// Nom de la propriété qui a changé. 
    /// Récupéré automatiquement via [CallerMemberName].
    /// </param>
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}