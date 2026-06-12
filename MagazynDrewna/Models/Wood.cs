using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MagazynDrewna.Models
{
    public class Wood : INotifyPropertyChanged
    {
        private int _id;
        private string _nazwa;
        private string _gatunek;
        private double _dlugosc;
        private int _ilosc;
        private string _lokalizacja;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Nazwa
        {
            get => _nazwa;
            set => SetProperty(ref _nazwa, value);
        }

        public string Gatunek
        {
            get => _gatunek;
            set => SetProperty(ref _gatunek, value);
        }

        public double Dlugosc
        {
            get => _dlugosc;
            set => SetProperty(ref _dlugosc, value);
        }

        public int Ilosc
        {
            get => _ilosc;
            set => SetProperty(ref _ilosc, value);
        }

        public string Lokalizacja
        {
            get => _lokalizacja;
            set => SetProperty(ref _lokalizacja, value);
        }

        public double MetryBiezace => Dlugosc * Ilosc;

        public Wood Clone()
        {
            return new Wood
            {
                Id = Id,
                Nazwa = Nazwa,
                Gatunek = Gatunek,
                Dlugosc = Dlugosc,
                Ilosc = Ilosc,
                Lokalizacja = Lokalizacja
            };
        }

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
