using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MagazynDrewna
{
    public class Wood : INotifyPropertyChanged
    {
        private int _id;
        private string _nazwa;
        private string _gatunek;
        private double _dlugosc;
        private int _ilosc;
        private string _lokalizacja;

        public int Id
        {
            get => _id;
            set { if (_id == value) return; _id = value; OnPropertyChanged(); }
        }

        public string Nazwa
        {
            get => _nazwa;
            set { if (_nazwa == value) return; _nazwa = value; OnPropertyChanged(); }
        }

        public string Gatunek
        {
            get => _gatunek;
            set { if (_gatunek == value) return; _gatunek = value; OnPropertyChanged(); }
        }

        public double Dlugosc
        {
            get => _dlugosc;
            set { if (_dlugosc == value) return; _dlugosc = value; OnPropertyChanged(); }
        }

        public int Ilosc
        {
            get => _ilosc;
            set { if (_ilosc == value) return; _ilosc = value; OnPropertyChanged(); }
        }

        public string Lokalizacja
        {
            get => _lokalizacja;
            set { if (_lokalizacja == value) return; _lokalizacja = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
