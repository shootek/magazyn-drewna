using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using MagazynDrewna.Services;

namespace MagazynDrewna.Views
{
    public partial class PodsumowaniaView : UserControl
    {
        private PodsumowanieMagazynu _lastMagazyn;
        private PodsumowanieDostaw _lastDostawy;

        public PodsumowaniaView()
        {
            InitializeComponent();
            Loaded += (_, __) => Refresh();
        }

        public void Refresh()
        {
            try
            {
                var woods = AppServices.Inventory.LoadWoods(seedIfEmpty: false);
                var deliveries = AppServices.Delivery.LoadDeliveries();

                _lastMagazyn = AppServices.Reports.BuildMagazynSummary(woods);
                TotalSztukText.Text = _lastMagazyn.LacznaIloscSztuk.ToString("N0", CultureInfo.CurrentCulture);
                TotalMbText.Text = _lastMagazyn.LaczneMetryBiezace.ToString("N1", CultureInfo.CurrentCulture);
                TotalPozycjiText.Text = _lastMagazyn.LiczbaPozycji.ToString(CultureInfo.CurrentCulture);
                TotalGatunkowText.Text = _lastMagazyn.LiczbaGatunkow.ToString(CultureInfo.CurrentCulture);

                ByGatunekGrid.ItemsSource = _lastMagazyn.WedlugGatunku;
                ByLokalizacjaGrid.ItemsSource = _lastMagazyn.WedlugLokalizacji;
                ByNazwaGrid.ItemsSource = _lastMagazyn.WedlugNazwy;
                LowStockGrid.ItemsSource = _lastMagazyn.NiskiStan;

                _lastDostawy = AppServices.Reports.BuildDeliverySummary(
                    deliveries, OdDatePicker.SelectedDate, DoDatePicker.SelectedDate);

                DeliveryCountText.Text = _lastDostawy.LiczbaDostaw.ToString(CultureInfo.CurrentCulture);
                DeliverySztukText.Text = _lastDostawy.LacznaIloscSztuk.ToString("N0", CultureInfo.CurrentCulture);
                DeliveryMbText.Text = _lastDostawy.LaczneMetryBiezace.ToString("N1", CultureInfo.CurrentCulture);
                ByDostawcaGrid.ItemsSource = _lastDostawy.WedlugDostawcy;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nie udało się wygenerować podsumowania:\n\n" + ex.Message,
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e) => Refresh();

        private void DateFilter_Changed(object sender, SelectionChangedEventArgs e) => Refresh();

        private void ExportCsv_Click(object sender, RoutedEventArgs e)
        {
            if (_lastMagazyn == null || _lastDostawy == null)
            {
                MessageBox.Show("Brak danych do eksportu. Odśwież podsumowanie.", "Eksport CSV",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!ExportDialog.TryPickSavePath("podsumowanie.csv", out var filePath))
            {
                return;
            }

            try
            {
                AppServices.Export.ExportSummary(_lastMagazyn, _lastDostawy, filePath);
                MessageBox.Show($"Raport zapisano do pliku:\n{filePath}", "Eksport CSV",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eksport nie powiódł się:\n\n" + ex.Message, "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
