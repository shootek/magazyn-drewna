using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MagazynDrewna.Models;

namespace MagazynDrewna.Views
{
    public partial class DostawyView : UserControl
    {
        public static readonly RoutedUICommand NewDeliveryCommand = new RoutedUICommand("Nowa dostawa", nameof(NewDeliveryCommand), typeof(DostawyView));
        public static readonly RoutedUICommand EditDeliveryCommand = new RoutedUICommand("Edytuj", nameof(EditDeliveryCommand), typeof(DostawyView));
        public static readonly RoutedUICommand AddItemCommand = new RoutedUICommand("Dodaj pozycję", nameof(AddItemCommand), typeof(DostawyView));
        public static readonly RoutedUICommand RemoveItemCommand = new RoutedUICommand("Usuń pozycję", nameof(RemoveItemCommand), typeof(DostawyView));
        public static readonly RoutedUICommand SaveDeliveryCommand = new RoutedUICommand("Zatwierdź", nameof(SaveDeliveryCommand), typeof(DostawyView));
        public static readonly RoutedUICommand CancelDeliveryCommand = new RoutedUICommand("Anuluj", nameof(CancelDeliveryCommand), typeof(DostawyView));
        public static readonly RoutedUICommand CompleteDeliveryCommand = new RoutedUICommand("Przyjmij do magazynu", nameof(CompleteDeliveryCommand), typeof(DostawyView));

        private readonly List<Dostawa> _deliveries = new List<Dostawa>();
        private readonly List<PozycjaDostawy> _draftItems = new List<PozycjaDostawy>();
        private bool _isEditing;
        private int _editingDeliveryId;
        public DostawyView()
        {
            InitializeComponent();
            WireCommands();
            PopulateComboBoxes();
            LoadDeliveries();
        }

        public void ReloadData() => LoadDeliveries();

        private void WireCommands()
        {
            CommandBindings.Add(new CommandBinding(NewDeliveryCommand, (_, __) => BeginNewDelivery(), (_, e) => e.CanExecute = !_isEditing));
            CommandBindings.Add(new CommandBinding(EditDeliveryCommand, (_, __) => BeginEditDelivery(), (_, e) =>
            {
                var selected = DeliveriesGrid.SelectedItem as Dostawa;
                e.CanExecute = !_isEditing && selected != null && !selected.Zrealizowana;
            }));
            CommandBindings.Add(new CommandBinding(AddItemCommand, (_, __) => AddDraftItem(), (_, e) => e.CanExecute = _isEditing));
            CommandBindings.Add(new CommandBinding(RemoveItemCommand, (_, __) => RemoveDraftItem(), (_, e) => e.CanExecute = _isEditing && ItemsGrid.SelectedItem != null));
            CommandBindings.Add(new CommandBinding(SaveDeliveryCommand, (_, __) => SaveDelivery(), (_, e) => e.CanExecute = _isEditing && CanSaveCurrentDraft()));
            CommandBindings.Add(new CommandBinding(CancelDeliveryCommand, (_, __) => CancelEdit(), (_, e) => e.CanExecute = _isEditing));
            CommandBindings.Add(new CommandBinding(CompleteDeliveryCommand, (_, __) => CompleteSelectedDelivery(), (_, e) =>
            {
                var selected = DeliveriesGrid.SelectedItem as Dostawa;
                e.CanExecute = !_isEditing && selected != null && selected.MoznaPrzyjacDoMagazynu;
            }));
        }

        private void PopulateComboBoxes()
        {
            foreach (var gatunek in MagazynConstants.Gatunki)
            {
                ItemGatunekBox.Items.Add(new ComboBoxItem { Content = gatunek });
            }

            foreach (var lokalizacja in MagazynConstants.Lokalizacje)
            {
                ItemLokalizacjaBox.Items.Add(new ComboBoxItem { Content = lokalizacja });
            }

            ItemGatunekBox.SelectedIndex = 0;
            ItemLokalizacjaBox.SelectedIndex = 0;
        }

        private void LoadDeliveries()
        {
            try
            {
                _deliveries.Clear();
                _deliveries.AddRange(AppServices.Delivery.LoadDeliveries());
                DeliveriesGrid.ItemsSource = null;
                DeliveriesGrid.ItemsSource = _deliveries;

                if (_deliveries.Count > 0 && !_isEditing)
                {
                    DeliveriesGrid.SelectedIndex = 0;
                }
                else if (!_isEditing)
                {
                    ClearDetails();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nie udało się wczytać dostaw:\n\n" + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeliveriesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isEditing)
            {
                ShowDeliveryDetails(DeliveriesGrid.SelectedItem as Dostawa);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void ShowDeliveryDetails(Dostawa dostawa)
        {
            if (dostawa == null)
            {
                ClearDetails();
                return;
            }

            FormTitleText.Text = "Szczegóły dostawy";
            SetSelectedDate(DataPicker, dostawa.Data);
            DostawcaBox.Text = dostawa.Dostawca ?? string.Empty;
            NumerDokumentuBox.Text = dostawa.NumerDokumentu ?? string.Empty;
            UwagiBox.Text = dostawa.Uwagi ?? string.Empty;
            ItemsGrid.ItemsSource = dostawa.Pozycje;
            UpdateStatusInfo(dostawa);
            PreviewActionsPanel.Visibility = Visibility.Visible;
            CompleteDeliveryButton.Visibility = dostawa.MoznaPrzyjacDoMagazynu ? Visibility.Visible : Visibility.Collapsed;
            UpdateEditButtonsVisibility(dostawa);
        }

        private void ClearDetails()
        {
            FormTitleText.Text = "Szczegóły dostawy";
            DataPicker.SelectedDate = null;
            DostawcaBox.Text = string.Empty;
            NumerDokumentuBox.Text = string.Empty;
            UwagiBox.Text = string.Empty;
            ItemsGrid.ItemsSource = null;
            StatusInfoText.Text = string.Empty;
            PreviewActionsPanel.Visibility = Visibility.Collapsed;
            CompleteDeliveryButton.Visibility = Visibility.Collapsed;
            TopEditButton.Visibility = Visibility.Collapsed;
            PreviewEditButton.Visibility = Visibility.Collapsed;
        }

        private void UpdateEditButtonsVisibility(Dostawa dostawa)
        {
            var canEdit = dostawa != null && !dostawa.Zrealizowana;
            TopEditButton.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
            PreviewEditButton.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BeginNewDelivery()
        {
            _isEditing = true;
            _editingDeliveryId = 0;
            _draftItems.Clear();
            FormTitleText.Text = "Nowa dostawa";
            SetSelectedDate(DataPicker, DateTime.Today);
            DostawcaBox.Text = string.Empty;
            NumerDokumentuBox.Text = string.Empty;
            UwagiBox.Text = string.Empty;
            ItemsGrid.ItemsSource = _draftItems;
            SetFormEnabled(true);
            EditPanel.Visibility = Visibility.Visible;
            PreviewActionsPanel.Visibility = Visibility.Collapsed;
            DeliveriesGrid.IsEnabled = false;
            ClearItemForm();
            UpdateDraftStatusInfo();
            UpdateSaveButtonCaption();
            CommandManager.InvalidateRequerySuggested();
        }

        private void BeginEditDelivery()
        {
            var selected = DeliveriesGrid.SelectedItem as Dostawa;
            if (selected == null)
            {
                return;
            }

            if (selected.Zrealizowana)
            {
                MessageBox.Show(
                    "Zrealizowanej dostawy nie można edytować.",
                    "Edycja niedostępna",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            _isEditing = true;
            _editingDeliveryId = selected.Id;
            _draftItems.Clear();

            foreach (var pozycja in selected.Pozycje)
            {
                _draftItems.Add(new PozycjaDostawy
                {
                    Nazwa = pozycja.Nazwa,
                    Gatunek = pozycja.Gatunek,
                    Dlugosc = pozycja.Dlugosc,
                    Ilosc = pozycja.Ilosc,
                    Lokalizacja = pozycja.Lokalizacja
                });
            }

            FormTitleText.Text = "Edycja dostawy";
            SetSelectedDate(DataPicker, selected.Data);
            DostawcaBox.Text = selected.Dostawca ?? string.Empty;
            NumerDokumentuBox.Text = selected.NumerDokumentu ?? string.Empty;
            UwagiBox.Text = selected.Uwagi ?? string.Empty;
            ItemsGrid.ItemsSource = _draftItems;
            SetFormEnabled(true);
            EditPanel.Visibility = Visibility.Visible;
            PreviewActionsPanel.Visibility = Visibility.Collapsed;
            DeliveriesGrid.IsEnabled = false;
            ClearItemForm();
            UpdateDraftStatusInfo();
            UpdateSaveButtonCaption();
            CommandManager.InvalidateRequerySuggested();
        }

        private void SetFormEnabled(bool enabled)
        {
            DataPicker.IsEnabled = enabled;
            DostawcaBox.IsEnabled = enabled;
            NumerDokumentuBox.IsEnabled = enabled;
            UwagiBox.IsEnabled = enabled;
        }

        private void AddDraftItem()
        {
            if (!TryReadItemForm(out var pozycja))
            {
                return;
            }

            _draftItems.Add(pozycja);
            ItemsGrid.ItemsSource = null;
            ItemsGrid.ItemsSource = _draftItems;
            ItemsGrid.SelectedItem = pozycja;
            ClearItemForm();
        }

        private void RemoveDraftItem()
        {
            var selected = ItemsGrid.SelectedItem as PozycjaDostawy;
            if (selected == null)
            {
                return;
            }

            _draftItems.Remove(selected);
            ItemsGrid.ItemsSource = null;
            ItemsGrid.ItemsSource = _draftItems;
        }

        private void SaveDelivery()
        {
            var dostawca = DostawcaBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(dostawca))
            {
                MessageBox.Show("Dostawca jest wymagany.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_draftItems.Any())
            {
                MessageBox.Show("Dodaj co najmniej jedną pozycję dostawy.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var data = GetSelectedDate(DataPicker) ?? DateTime.Today;
            var dostawa = new Dostawa
            {
                Data = data,
                Dostawca = dostawca,
                NumerDokumentu = NumerDokumentuBox.Text.Trim(),
                Uwagi = UwagiBox.Text.Trim(),
                Pozycje = _draftItems.ToList()
            };

            try
            {
                var woods = AppServices.Inventory.LoadWoods(seedIfEmpty: false);

                if (_editingDeliveryId > 0)
                {
                    dostawa.Id = _editingDeliveryId;
                    var original = _deliveries.FirstOrDefault(d => d.Id == _editingDeliveryId);
                    if (original == null)
                    {
                        MessageBox.Show("Nie znaleziono edytowanej dostawy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    AppServices.Delivery.UpdateDelivery(original, dostawa, woods);
                }
                else
                {
                    AppServices.Delivery.RegisterDelivery(dostawa, woods);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    (_editingDeliveryId > 0 ? "Zapis zmian nie powiódł się:\n\n" : "Rejestracja dostawy nie powiodła się:\n\n") + ex.Message,
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var savedId = dostawa.Id;
            var wasEdit = _editingDeliveryId > 0;
            var becameCompleted = dostawa.Zrealizowana;

            FinishEdit();
            LoadDeliveries();
            DeliveriesGrid.SelectedItem = _deliveries.FirstOrDefault(d => d.Id == savedId);

            if (wasEdit)
            {
                MessageBox.Show(
                    becameCompleted
                        ? "Dostawa została zaktualizowana i przyjęta do magazynu."
                        : "Dostawa została zaktualizowana.",
                    "Sukces",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else if (dostawa.Zrealizowana)
            {
                MessageBox.Show("Dostawa została przyjęta do magazynu.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    $"Dostawa zaplanowana na {dostawa.Data:d}.\nPrzyjęcie do magazynu będzie możliwe w dniu dostawy.",
                    "Zaplanowano dostawę",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void CompleteSelectedDelivery()
        {
            var selected = DeliveriesGrid.SelectedItem as Dostawa;
            if (selected == null)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Przyjąć dostawę z dnia {selected.Data:d} do magazynu?",
                "Potwierdzenie",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var woods = AppServices.Inventory.LoadWoods(seedIfEmpty: false);
                AppServices.Delivery.CompletePlannedDelivery(selected, woods);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Nie można przyjąć dostawy", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LoadDeliveries();
            DeliveriesGrid.SelectedItem = _deliveries.FirstOrDefault(d => d.Id == selected.Id);
            MessageBox.Show("Dostawa została przyjęta do magazynu.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CancelEdit()
        {
            var restoreId = _editingDeliveryId;
            FinishEdit();

            var dostawa = restoreId > 0
                ? _deliveries.FirstOrDefault(d => d.Id == restoreId)
                : _deliveries.FirstOrDefault();

            if (dostawa != null)
            {
                DeliveriesGrid.SelectedItem = dostawa;
                ShowDeliveryDetails(dostawa);
            }
            else
            {
                ClearDetails();
            }
        }

        private void FinishEdit()
        {
            _isEditing = false;
            _editingDeliveryId = 0;
            _draftItems.Clear();
            SetFormEnabled(false);
            EditPanel.Visibility = Visibility.Collapsed;
            DeliveriesGrid.IsEnabled = true;
            CommandManager.InvalidateRequerySuggested();
        }

        private bool CanSaveCurrentDraft()
        {
            var data = GetSelectedDate(DataPicker);
            return data.HasValue;
        }

        private void DataPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isEditing)
            {
                UpdateDraftStatusInfo();
                UpdateSaveButtonCaption();
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private void UpdateDraftStatusInfo()
        {
            var data = GetSelectedDate(DataPicker);
            if (!data.HasValue)
            {
                StatusInfoText.Text = "Wybierz datę dostawy.";
                StatusInfoText.Foreground = System.Windows.Media.Brushes.DarkOrange;
                return;
            }

            if (data.Value.Date > DateTime.Today)
            {
                StatusInfoText.Text = $"Dostawa zostanie zaplanowana na {data.Value:d}. Magazyn zaktualizuje się dopiero w tym dniu.";
                StatusInfoText.Foreground = System.Windows.Media.Brushes.DarkOrange;
            }
            else
            {
                StatusInfoText.Text = "Dostawa zostanie od razu przyjęta do magazynu.";
                StatusInfoText.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void UpdateSaveButtonCaption()
        {
            if (_editingDeliveryId > 0)
            {
                SaveDeliveryButton.Content = "Zapisz zmiany";
                return;
            }

            var data = GetSelectedDate(DataPicker);
            SaveDeliveryButton.Content = data.HasValue && data.Value.Date > DateTime.Today
                ? "Zapisz zaplanowaną"
                : "Zatwierdź dostawę";
        }

        private void UpdateStatusInfo(Dostawa dostawa)
        {
            if (dostawa.Zrealizowana)
            {
                StatusInfoText.Text = "Status: zrealizowana — towar jest w magazynie. Edycja jest zablokowana.";
                StatusInfoText.Foreground = System.Windows.Media.Brushes.Gray;
                return;
            }

            if (dostawa.Data.Date > DateTime.Today)
            {
                StatusInfoText.Text = $"Status: zaplanowana na {dostawa.Data:d}. Przyjęcie do magazynu będzie możliwe w dniu dostawy.";
                StatusInfoText.Foreground = System.Windows.Media.Brushes.DarkOrange;
            }
            else
            {
                StatusInfoText.Text = "Status: oczekuje na przyjęcie — możesz przyjąć dostawę do magazynu.";
                StatusInfoText.Foreground = System.Windows.Media.Brushes.DarkGreen;
            }
        }

        private static void SetSelectedDate(DatePicker picker, DateTime date)
        {
            picker.SelectedDate = null;
            picker.SelectedDate = date.Date;
            picker.DisplayDate = date.Date;
        }

        private static DateTime? GetSelectedDate(DatePicker picker)
        {
            return picker.SelectedDate?.Date;
        }

        private bool TryReadItemForm(out PozycjaDostawy pozycja)
        {
            pozycja = null;
            var nazwa = ItemNazwaBox.Text.Trim();
            var gatunek = ((ComboBoxItem)ItemGatunekBox.SelectedItem)?.Content?.ToString() ?? string.Empty;
            var lokalizacja = ((ComboBoxItem)ItemLokalizacjaBox.SelectedItem)?.Content?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(nazwa))
            {
                MessageBox.Show("Nazwa pozycji jest wymagana.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var dlugoscText = ItemDlugoscBox.Text.Trim().Replace(',', '.');
            if (!double.TryParse(dlugoscText, NumberStyles.Float, CultureInfo.InvariantCulture, out var dlugosc) || dlugosc <= 0)
            {
                MessageBox.Show("Długość musi być liczbą większą od 0.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(ItemIloscBox.Text.Trim(), out var ilosc) || ilosc <= 0)
            {
                MessageBox.Show("Ilość musi być liczbą całkowitą większą od 0.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            pozycja = new PozycjaDostawy
            {
                Nazwa = nazwa,
                Gatunek = gatunek,
                Dlugosc = dlugosc,
                Ilosc = ilosc,
                Lokalizacja = lokalizacja
            };

            return true;
        }

        private void ClearItemForm()
        {
            ItemNazwaBox.Text = string.Empty;
            ItemGatunekBox.SelectedIndex = 0;
            ItemDlugoscBox.Text = string.Empty;
            ItemIloscBox.Text = string.Empty;
            ItemLokalizacjaBox.SelectedIndex = 0;
        }
    }
}
