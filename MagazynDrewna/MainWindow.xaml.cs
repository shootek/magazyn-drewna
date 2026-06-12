using System.Windows;
using System.Windows.Controls;

namespace MagazynDrewna
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source != MainTabs || MainTabs.SelectedItem == null)
            {
                return;
            }

            if (MainTabs.SelectedItem == MagazynTabItem)
            {
                MagazynView.ReloadData();
            }
            else if (MainTabs.SelectedItem == DostawyTabItem)
            {
                DostawyView.ReloadData();
            }
            else if (MainTabs.SelectedItem == PodsumowaniaTabItem)
            {
                PodsumowaniaView.Refresh();
            }
        }
    }
}
