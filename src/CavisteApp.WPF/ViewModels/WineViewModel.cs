using CavisteApp.WPF.Services;
using CavisteApp.WPF.ViewModels;
using System.Collections.ObjectModel;

namespace CavisteApp.WPF.ViewModels
{
    public class VinDto
    {
        public string Nom { get; set; } = "";
        public string Region { get; set; } = "";
        public string Couleur { get; set; } = "";
        public int Annee { get; set; }
        public decimal Prix { get; set; }
    }

    public class WineViewModel : ViewModelBase
    {
        private readonly SessionService _session;

        public WineViewModel(SessionService session)
        {
            _session = session;

            // Données de test — à remplacer par un appel API
            _tousLesVins = new ObservableCollection<VinDto>
            {
                new() { Nom = "Château Margaux", Region = "Bordeaux", Couleur = "Rouge", Annee = 2018, Prix = 250 },
                new() { Nom = "Pouilly-Fumé", Region = "Loire", Couleur = "Blanc", Annee = 2021, Prix = 35 },
                new() { Nom = "Côtes du Rhône", Region = "Rhône", Couleur = "Rouge", Annee = 2020, Prix = 15 },
                new() { Nom = "Sancerre", Region = "Loire", Couleur = "Blanc", Annee = 2022, Prix = 28 },
            };
            Vins = new ObservableCollection<VinDto>(_tousLesVins);
        }

        private ObservableCollection<VinDto> _vins = new();
        public ObservableCollection<VinDto> Vins
        {
            get => _vins;
            set => SetProperty(ref _vins, value);
        }

        private VinDto? _vinSelectionne;
        public VinDto? VinSelectionne
        {
            get => _vinSelectionne;
            set => SetProperty(ref _vinSelectionne, value);
        }

        private string _recherche = "";
        public string Recherche
        {
            get => _recherche;
            set
            {
                SetProperty(ref _recherche, value);
                Filtrer();
            }
        }

        private ObservableCollection<VinDto> _tousLesVins = new();

        private void Filtrer()
        {
            var filtre = Recherche.ToLower();
            var resultats = _tousLesVins
                .Where(v => v.Nom.ToLower().Contains(filtre) ||
                            v.Region.ToLower().Contains(filtre) ||
                            v.Couleur.ToLower().Contains(filtre))
                .ToList();
            Vins = new ObservableCollection<VinDto>(resultats);
        }
    }
}