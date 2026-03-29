using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using MagazynDrewna;

namespace MagazynDrewna.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private Wood _selectedWood;

        public MainViewModel()
        {
            Woods = new ObservableCollection<Wood>
            {
                new Wood { Id = 1, Nazwa = "Sosna", Gatunek = "Iglaste", Dlugosc = 2.5, Ilosc = 30, Lokalizacja = "Magazyn A" },
                new Wood { Id = 2, Nazwa = "Dąb", Gatunek = "Liściaste", Dlugosc = 3.0, Ilosc = 15, Lokalizacja = "Magazyn B" },
                new Wood { Id = 3, Nazwa = "Świerk", Gatunek = "Iglaste", Dlugosc = 2.0, Ilosc = 50, Lokalizacja = "Magazyn C" },
                new Wood { Id = 4, Nazwa = "Buk", Gatunek = "Liściaste", Dlugosc = 3.0, Ilosc = 20, Lokalizacja = "Magazyn D" }
            };

            AddWoodCommand = new RelayCommand(_ => AddWood());
            DeleteWoodCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedWood != null);
        }

        public string[] Gatunki { get; } = { "Iglaste", "Liściaste" };

        public ObservableCollection<Wood> Woods { get; }

        public Wood SelectedWood
        {
            get => _selectedWood;
            set
            {
                if (!ReferenceEquals(_selectedWood, value))
                {
                    _selectedWood = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ICommand AddWoodCommand { get; }

        public ICommand DeleteWoodCommand { get; }

        private int NextId()
        {
            return Woods.Count == 0 ? 1 : Woods.Max(w => w.Id) + 1;
        }

        private void AddWood()
        {
            var wood = new Wood
            {
                Id = NextId(),
                Nazwa = string.Empty,
                Gatunek = Gatunki[0],
                Dlugosc = 0,
                Ilosc = 0,
                Lokalizacja = string.Empty
            };
            Woods.Add(wood);
            SelectedWood = wood;
        }

        private void DeleteSelected()
        {
            if (SelectedWood == null) return;

            var index = Woods.IndexOf(SelectedWood);
            Woods.Remove(SelectedWood);
            SelectedWood = index > 0 ? Woods[index - 1] : Woods.FirstOrDefault();
        }
    }
}
