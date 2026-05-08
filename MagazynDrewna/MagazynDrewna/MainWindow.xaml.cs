using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using System.ComponentModel;

namespace MagazynDrewna
{

    public partial class MainWindow : Window
    {
        public static readonly RoutedUICommand NewCommand = new RoutedUICommand("Nowy", nameof(NewCommand), typeof(MainWindow));
        public static readonly RoutedUICommand EditCommand = new RoutedUICommand("Edytuj", nameof(EditCommand), typeof(MainWindow));
        public static readonly RoutedUICommand DeleteCommand = new RoutedUICommand("Usuń", nameof(DeleteCommand), typeof(MainWindow));
        public static readonly RoutedUICommand SaveCommand = new RoutedUICommand("Zapisz", nameof(SaveCommand), typeof(MainWindow));
        public static readonly RoutedUICommand CancelCommand = new RoutedUICommand("Anuluj", nameof(CancelCommand), typeof(MainWindow));
        public static readonly RoutedUICommand ClearFiltersCommand = new RoutedUICommand("Wyczyść filtry", nameof(ClearFiltersCommand), typeof(MainWindow));

        private readonly List<Wood> _woods = new List<Wood>();
        private readonly SQLiteBaza _storage = new SQLiteBaza();
        private ICollectionView _woodsView;
        private bool _isEditing;
        private bool _isAdding;

        public MainWindow()
        {
            InitializeComponent();
            WireCommands();
            LoadData();
        }

        private void WireCommands()
        {
            CommandBindings.Add(new CommandBinding(NewCommand, (_, __) => BeginAdd(), (_, e) => e.CanExecute = !_isEditing));
            CommandBindings.Add(new CommandBinding(EditCommand, (_, __) => BeginEdit(), (_, e) => e.CanExecute = !_isEditing && WoodGrid.SelectedItem != null));
            CommandBindings.Add(new CommandBinding(DeleteCommand, (_, __) => DeleteSelected(), (_, e) => e.CanExecute = !_isEditing && WoodGrid.SelectedItem != null));
            CommandBindings.Add(new CommandBinding(SaveCommand, (_, __) => SaveCurrent(), (_, e) => e.CanExecute = _isEditing));
            CommandBindings.Add(new CommandBinding(CancelCommand, (_, __) => EnterPreviewMode(), (_, e) => e.CanExecute = _isEditing));
            CommandBindings.Add(new CommandBinding(ClearFiltersCommand, (_, __) => ClearFilters()));
        }

        private void LoadData()
        {
            try
            {
                _storage.Initialize();
                _woods.Clear();
                _woods.AddRange(_storage.LoadAll());

                if (_woods.Count == 0)
                {
                    LoadDefaultData();
                    SaveData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Nie udało się odczytać pliku danych. Wczytano dane testowe.\n\n" + ex.Message,
                    "Błąd odczytu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                _woods.Clear();
                LoadDefaultData();
            }

            _woodsView = CollectionViewSource.GetDefaultView(_woods);
            _woodsView.Filter = FilterWood;
            WoodGrid.ItemsSource = _woodsView;
            ApplyFiltersAndSorting();

            if (_woods.Count > 0)
            {
                WoodGrid.SelectedItem = _woods[0];
            }
            EnterPreviewMode();
        }

        private void LoadDefaultData()
        {
            _woods.AddRange(new List<Wood>
            {
                new Wood{ Id=1, Nazwa="Sosna", Gatunek="Iglaste", Dlugosc=2.5, Ilosc=30, Lokalizacja="Magazyn A"},
                new Wood{ Id=2, Nazwa="Dąb", Gatunek="Liściaste", Dlugosc=3.0, Ilosc=15, Lokalizacja="Magazyn B"},
                new Wood{ Id=3, Nazwa="Świerk", Gatunek="Iglaste", Dlugosc=2.0, Ilosc=50, Lokalizacja="Magazyn C"},
                new Wood{ Id=4, Nazwa="Buk", Gatunek="Liściaste", Dlugosc=3.0, Ilosc=20, Lokalizacja="Magazyn D"},
            });
        }

        private void SaveData()
        {
            _storage.SaveAll(_woods);
        }

        private void EnterPreviewMode()
        {
            _isEditing = false;
            _isAdding = false;

            FormTitleText.Text = "Formularz - podgląd";
            PreviewButtonsPanel.Visibility = Visibility.Visible;
            EditButtonsPanel.Visibility = Visibility.Collapsed;
            WoodGrid.IsEnabled = true;
            SetFormEnabled(false);
            FillFormFromSelection();
        }

        private void EnterEditMode(bool adding)
        {
            _isEditing = true;
            _isAdding = adding;

            FormTitleText.Text = adding ? "Formularz - dodawanie" : "Formularz - edycja";
            PreviewButtonsPanel.Visibility = Visibility.Collapsed;
            EditButtonsPanel.Visibility = Visibility.Visible;
            WoodGrid.IsEnabled = false;
            SetFormEnabled(true);

            if (adding)
            {
                ClearForm();
            }
            else
            {
                FillFormFromSelection();
            }
        }

        private void SetFormEnabled(bool enabled)
        {
            NazwaBox.IsEnabled = enabled;
            GatunekBox.IsEnabled = enabled;
            DlugoscBox.IsEnabled = enabled;
            IloscBox.IsEnabled = enabled;
            LokalizacjaBox.IsEnabled = enabled;
        }

        private void FillFormFromSelection()
        {
            var selected = WoodGrid.SelectedItem as Wood;
            if (selected == null)
            {
                ClearForm();
                return;
            }

            NazwaBox.Text = selected.Nazwa ?? string.Empty;
            GatunekBox.SelectedIndex = selected.Gatunek == "Liściaste" ? 1 : 0;
            DlugoscBox.Text = selected.Dlugosc.ToString();
            IloscBox.Text = selected.Ilosc.ToString();
            SetLokalizacjaSelection(selected.Lokalizacja);
        }

        private void ClearForm()
        {
            NazwaBox.Text = string.Empty;
            GatunekBox.SelectedIndex = 0;
            DlugoscBox.Text = string.Empty;
            IloscBox.Text = string.Empty;
            LokalizacjaBox.SelectedIndex = 0;
        }

        private void WoodGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isEditing)
            {
                FillFormFromSelection();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void BeginAdd()
        {
            EnterEditMode(true);
        }

        private void BeginEdit()
        {
            if (WoodGrid.SelectedItem == null)
            {
                MessageBox.Show("Wybierz element do edycji.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            EnterEditMode(false);
        }

        private void SaveCurrent()
        {
            if (!TryReadFormValues(out var nazwa, out var gatunek, out var dlugosc, out var ilosc, out var lokalizacja))
            {
                return;
            }

            if (_isAdding)
            {
                var existing = _woods.FirstOrDefault(w =>
                    string.Equals(w.Nazwa, nazwa, StringComparison.CurrentCultureIgnoreCase) &&
                    string.Equals(w.Gatunek, gatunek, StringComparison.CurrentCultureIgnoreCase) &&
                    string.Equals(w.Lokalizacja, lokalizacja, StringComparison.CurrentCultureIgnoreCase) &&
                    Math.Abs(w.Dlugosc - dlugosc) < 0.0001);

                if (existing != null)
                {
                    existing.Ilosc += ilosc;
                    WoodGrid.SelectedItem = existing;
                }
                else
                {
                    var nextId = _woods.Count == 0 ? 1 : _woods.Max(w => w.Id) + 1;
                    var newWood = new Wood
                    {
                        Id = nextId,
                        Nazwa = nazwa,
                        Gatunek = gatunek,
                        Dlugosc = dlugosc,
                        Ilosc = ilosc,
                        Lokalizacja = lokalizacja
                    };
                    _woods.Add(newWood);
                    WoodGrid.SelectedItem = newWood;
                }

                _woodsView.Refresh();
            }
            else
            {
                var selected = WoodGrid.SelectedItem as Wood;
                if (selected == null)
                {
                    MessageBox.Show("Nie wybrano elementu do edycji.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                selected.Nazwa = nazwa;
                selected.Gatunek = gatunek;
                selected.Dlugosc = dlugosc;
                selected.Ilosc = ilosc;
                selected.Lokalizacja = lokalizacja;
                _woodsView.Refresh();
            }

            try
            {
                SaveData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Zapis danych nie powiódł się:\n\n" + ex.Message, "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            EnterPreviewMode();
        }

        private bool TryReadFormValues(out string nazwa, out string gatunek, out double dlugosc, out int ilosc, out string lokalizacja)
        {
            nazwa = NazwaBox.Text.Trim();
            gatunek = ((ComboBoxItem)GatunekBox.SelectedItem)?.Content?.ToString() ?? string.Empty;
            lokalizacja = ((ComboBoxItem)LokalizacjaBox.SelectedItem)?.Content?.ToString() ?? string.Empty;
            dlugosc = 0;
            ilosc = 0;

            if (string.IsNullOrWhiteSpace(nazwa))
            {
                MessageBox.Show("Nazwa jest wymagana.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var dlugoscText = DlugoscBox.Text.Trim();
            var dlugoscNormalized = dlugoscText.Replace(',', '.');
            var parsedDlugosc =
                double.TryParse(dlugoscText, NumberStyles.Float, CultureInfo.CurrentCulture, out dlugosc) ||
                double.TryParse(dlugoscNormalized, NumberStyles.Float, CultureInfo.InvariantCulture, out dlugosc);

            if (!parsedDlugosc || dlugosc <= 0)
            {
                MessageBox.Show("Długość musi być liczbą większą od 0.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(IloscBox.Text.Trim(), out ilosc) || ilosc <= 0)
            {
                MessageBox.Show("Ilość musi być liczbą całkowitą większą od 0.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(lokalizacja))
            {
                MessageBox.Show("Lokalizacja jest wymagana.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void SetLokalizacjaSelection(string lokalizacja)
        {
            foreach (var item in LokalizacjaBox.Items)
            {
                var comboItem = item as ComboBoxItem;
                if (comboItem != null && string.Equals(comboItem.Content?.ToString(), lokalizacja, StringComparison.CurrentCultureIgnoreCase))
                {
                    LokalizacjaBox.SelectedItem = comboItem;
                    return;
                }
            }

            LokalizacjaBox.SelectedIndex = 0;
        }

        private void DeleteSelected()
        {
            var selected = WoodGrid.SelectedItem as Wood;
            if (selected == null)
            {
                MessageBox.Show("Wybierz element do usunięcia.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                "Czy na pewno chcesz usunąć wybrany element?",
                "Potwierdzenie usuwania",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            var index = _woods.IndexOf(selected);
            _woods.Remove(selected);
            _woodsView.Refresh();

            try
            {
                SaveData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Zapis danych nie powiódł się:\n\n" + ex.Message, "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (_woods.Count > 0)
            {
                var safeIndex = Math.Min(index, _woods.Count - 1);
                WoodGrid.SelectedItem = _woods[safeIndex];
            }
            else
            {
                ClearForm();
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private bool FilterWood(object obj)
        {
            var wood = obj as Wood;
            if (wood == null)
            {
                return false;
            }

            var search = SearchNameBox.Text.Trim();
            var gatunek = ((ComboBoxItem)FilterGatunekBox.SelectedItem)?.Content?.ToString() ?? "Wszystkie";
            var lokalizacja = ((ComboBoxItem)FilterLokalizacjaBox.SelectedItem)?.Content?.ToString() ?? "Wszystkie";

            bool matchesName = string.IsNullOrWhiteSpace(search) ||
                               (wood.Nazwa?.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0);
            bool matchesGatunek = gatunek == "Wszystkie" || wood.Gatunek == gatunek;
            bool matchesLokalizacja = lokalizacja == "Wszystkie" || wood.Lokalizacja == lokalizacja;

            return matchesName && matchesGatunek && matchesLokalizacja;
        }

        private void ApplyFiltersAndSorting()
        {
            if (_woodsView == null)
            {
                return;
            }

            _woodsView.SortDescriptions.Clear();

            var sortBy = ((ComboBoxItem)SortByBox.SelectedItem)?.Content?.ToString() ?? "Sortuj: Nazwa";
            var direction = ((ComboBoxItem)SortDirectionBox.SelectedItem)?.Content?.ToString() == "Malejąco"
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;

            if (sortBy.Contains("Długość"))
            {
                _woodsView.SortDescriptions.Add(new SortDescription(nameof(Wood.Dlugosc), direction));
            }
            else if (sortBy.Contains("Ilość"))
            {
                _woodsView.SortDescriptions.Add(new SortDescription(nameof(Wood.Ilosc), direction));
            }
            else
            {
                _woodsView.SortDescriptions.Add(new SortDescription(nameof(Wood.Nazwa), direction));
            }

            _woodsView.Refresh();
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            ApplyFiltersAndSorting();
        }

        private void ClearFilters()
        {
            SearchNameBox.Text = string.Empty;
            FilterGatunekBox.SelectedIndex = 0;
            FilterLokalizacjaBox.SelectedIndex = 0;
            SortByBox.SelectedIndex = 0;
            SortDirectionBox.SelectedIndex = 0;
            ApplyFiltersAndSorting();
        }
    }
}
