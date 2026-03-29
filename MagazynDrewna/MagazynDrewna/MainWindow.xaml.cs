using System.Windows;
using MagazynDrewna.ViewModels;

namespace MagazynDrewna
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
