using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MagazynDrewna
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadTestData();
        }
    private void LoadTestData()
        {
            var woods = new List<Wood>()
            {
                new Wood{ Id=1, Nazwa="Sosna", Gatunek="Iglaste", Dlugosc=2.5, Ilosc=30, Lokalizacja="Magazyn A"},
                new Wood{ Id=2, Nazwa="Dąb", Gatunek="Liściaste", Dlugosc=3.0, Ilosc=15, Lokalizacja="Magazyn B"},
                new Wood{ Id=3, Nazwa="Świerk", Gatunek="Iglaste", Dlugosc=2.0, Ilosc=50, Lokalizacja="Magazyn C"},
                new Wood{ Id=4, Nazwa="Buk", Gatunek="Liściaste", Dlugosc=3.0, Ilosc=20, Lokalizacja="Magazyn D"},

            };

            WoodGrid.ItemsSource = woods;
        }
    }
}
