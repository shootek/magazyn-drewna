using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using MagazynDrewna.Models;

namespace MagazynDrewna.Views
{
    public partial class MagazynView : UserControl
    {
        public static readonly RoutedUICommand NewCommand = new RoutedUICommand("Nowy", nameof(NewCommand), typeof(MagazynView));
        public static readonly RoutedUICommand EditCommand = new RoutedUICommand("Edytuj", nameof(EditCommand), typeof(MagazynView));
        public static readonly RoutedUICommand DeleteCommand = new RoutedUICommand("Usuń", nameof(DeleteCommand), typeof(MagazynView));
        public static readonly RoutedUICommand SaveCommand = new RoutedUICommand("Zapisz", nameof(SaveCommand), typeof(MagazynView));
        public static readonly RoutedUICommand CancelCommand = new RoutedUICommand("Anuluj", nameof(CancelCommand), typeof(MagazynView));
        public static readonly RoutedUICommand ClearFiltersCommand = new RoutedUICommand("Wyczyść filtry", nameof(ClearFiltersCommand), typeof(MagazynView));
        public static readonly RoutedUICommand ExportCsvCommand = new RoutedUICommand("Eksport CSV", nameof(ExportCsvCommand), typeof(MagazynView));
        public static readonly RoutedUICommand ImportCsvCommand = new RoutedUICommand("Import CSV", nameof(ImportCsvCommand), typeof(MagazynView));

        private readonly List<Wood> _woods = new List<Wood>();
        private ICollectionView _woodsView;
        private Wood _formWood;
        private bool _isEditing;
        private bool _isAdding;

        public MagazynView()
        {
            InitializeComponent();
            WireCommands();
            PopulateFilterComboBoxes();
            LoadData();
        }

        public void ReloadData()
        {
            try
            {
                _woods.Clear();
                _woods.AddRange(AppServices.Inventory.LoadWoods(seedIfEmpty: false));
                _woodsView?.Refresh();
                ApplyFiltersAndSorting();

                if (_woods.Count > 0 && WoodGrid.SelectedItem == null)
                {
                    WoodGrid.SelectedItem = _woods[0];
                }

                if (!_isEditing)
                {
                    BindFormFromSelection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nie udało się odświeżyć magazynu:\n\n" + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void WireCommands()
        {
            CommandBindings.Add(new CommandBinding(NewCommand, (_, __) => BeginAdd(), (_, e) => e.CanExecute = !_isEditing));
            CommandBindings.Add(new CommandBinding(EditCommand, (_, __) => BeginEdit(), (_, e) => e.CanExecute = !_isEditing && WoodGrid.SelectedItem != null));
            CommandBindings.Add(new CommandBinding(DeleteCommand, (_, __) => DeleteSelected(), (_, e) => e.CanExecute = !_isEditing && WoodGrid.SelectedItem != null));
            CommandBindings.Add(new CommandBinding(SaveCommand, (_, __) => SaveCurrent(), (_, e) => e.CanExecute = _isEditing));
            CommandBindings.Add(new CommandBinding(CancelCommand, (_, __) => EnterPreviewMode(), (_, e) => e.CanExecute = _isEditing));
            CommandBindings.Add(new CommandBinding(ClearFiltersCommand, (_, __) => ClearFilters()));
            CommandBindings.Add(new CommandBinding(ExportCsvCommand, (_, __) => ExportToCsv(), (_, e) => e.CanExecute = !_isEditing));
            CommandBindings.Add(new CommandBinding(ImportCsvCommand, (_, __) => ImportFromCsv(), (_, e) => e.CanExecute = !_isEditing));
        }

        private void PopulateFilterComboBoxes()
        {
            FilterGatunekBox.Items.Add(CreateComboItem("Wszystkie", true));
            foreach (var gatunek in MagazynConstants.Gatunki)
            {
                FilterGatunekBox.Items.Add(CreateComboItem(gatunek));
            }

            FilterLokalizacjaBox.Items.Add(CreateComboItem("Wszystkie", true));
            foreach (var lokalizacja in MagazynConstants.Lokalizacje)
            {
                FilterLokalizacjaBox.Items.Add(CreateComboItem(lokalizacja));
            }
        }

        private static ComboBoxItem CreateComboItem(string content, bool selected = false)
        {
            return new ComboBoxItem { Content = content, IsSelected = selected };
        }

        private void LoadData()
        {
            try
            {
                AppServices.Inventory.Initialize();
                _woods.Clear();
                _woods.AddRange(AppServices.Inventory.LoadWoods());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Nie udało się odczytać danych. Wczytano dane testowe.\n\n" + ex.Message,
                    "Błąd odczytu",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                _woods.Clear();
                _woods.AddRange(AppServices.Inventory.LoadWoods());
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

        private void BindForm(Wood wood)
        {
            _formWood = wood;
            FormFieldsPanel.DataContext = wood;
        }

        private void BindFormFromSelection()
        {
            var selected = WoodGrid.SelectedItem as Wood;
            BindForm(selected ?? CreateEmptyWood());
        }

        private static Wood CreateEmptyWood()
        {
            return new Wood
            {
                Gatunek = MagazynConstants.Gatunki[0],
                Lokalizacja = MagazynConstants.Lokalizacje[0]
            };
        }

        private void EnterPreviewMode()
        {
            _isEditing = false;
            _isAdding = false;
            FormTitleText.Text = "Formularz - podgląd";
            PreviewButtonsPanel.Visibility = Visibility.Visible;
            EditButtonsPanel.Visibility = Visibility.Collapsed;
            WoodGrid.IsEnabled = true;
            SetFormFieldsEnabled(false);
            BindFormFromSelection();
        }

        private void EnterEditMode(bool adding)
        {
            _isEditing = true;
            _isAdding = adding;
            FormTitleText.Text = adding ? "Formularz - dodawanie" : "Formularz - edycja";
            PreviewButtonsPanel.Visibility = Visibility.Collapsed;
            EditButtonsPanel.Visibility = Visibility.Visible;
            WoodGrid.IsEnabled = false;
            SetFormFieldsEnabled(true);

            if (adding)
            {
                BindForm(CreateEmptyWood());
            }
            else
            {
                var selected = WoodGrid.SelectedItem as Wood;
                BindForm(selected?.Clone() ?? CreateEmptyWood());
            }
        }

        private void SetFormFieldsEnabled(bool enabled)
        {
            FormFieldsPanel.IsEnabled = enabled;
        }

        private void WoodGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isEditing)
            {
                BindFormFromSelection();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private void BeginAdd() => EnterEditMode(true);

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
            if (_formWood == null || !TryValidateWood(_formWood, out var nazwa, out var gatunek, out var dlugosc, out var ilosc, out var lokalizacja))
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
                AppServices.Inventory.SaveWoods(_woods);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Zapis danych nie powiódł się:\n\n" + ex.Message, "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            EnterPreviewMode();
        }

        private bool TryValidateWood(Wood wood, out string nazwa, out string gatunek, out double dlugosc, out int ilosc, out string lokalizacja)
        {
            nazwa = wood.Nazwa?.Trim() ?? string.Empty;
            gatunek = wood.Gatunek?.Trim() ?? string.Empty;
            lokalizacja = wood.Lokalizacja?.Trim() ?? string.Empty;
            dlugosc = wood.Dlugosc;
            ilosc = wood.Ilosc;

            if (string.IsNullOrWhiteSpace(nazwa))
            {
                MessageBox.Show("Nazwa jest wymagana.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (dlugosc <= 0)
            {
                MessageBox.Show("Długość musi być liczbą większą od 0.", "Walidacja", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (ilosc <= 0)
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

        private void DeleteSelected()
        {
            var selected = WoodGrid.SelectedItem as Wood;
            if (selected == null)
            {
                MessageBox.Show("Wybierz element do usunięcia.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("Czy na pewno chcesz usunąć wybrany element?", "Potwierdzenie usuwania",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                return;
            }

            var index = _woods.IndexOf(selected);
            _woods.Remove(selected);
            _woodsView.Refresh();

            try
            {
                AppServices.Inventory.SaveWoods(_woods);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Zapis danych nie powiódł się:\n\n" + ex.Message, "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (_woods.Count > 0)
            {
                WoodGrid.SelectedItem = _woods[Math.Min(index, _woods.Count - 1)];
            }
            else
            {
                BindForm(CreateEmptyWood());
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
            var gatunek = GetComboText(FilterGatunekBox, "Wszystkie");
            var lokalizacja = GetComboText(FilterLokalizacjaBox, "Wszystkie");

            return (string.IsNullOrWhiteSpace(search) ||
                    wood.Nazwa?.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0) &&
                   (gatunek == "Wszystkie" || wood.Gatunek == gatunek) &&
                   (lokalizacja == "Wszystkie" || wood.Lokalizacja == lokalizacja);
        }

        private void ApplyFiltersAndSorting()
        {
            if (_woodsView == null)
            {
                return;
            }

            _woodsView.SortDescriptions.Clear();
            var sortBy = GetComboText(SortByBox, "Sortuj: Nazwa");
            var direction = GetComboText(SortDirectionBox) == "Malejąco"
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

        private void FilterChanged(object sender, EventArgs e) => ApplyFiltersAndSorting();

        private void ClearFilters()
        {
            SearchNameBox.Text = string.Empty;
            FilterGatunekBox.SelectedIndex = 0;
            FilterLokalizacjaBox.SelectedIndex = 0;
            SortByBox.SelectedIndex = 0;
            SortDirectionBox.SelectedIndex = 0;
            ApplyFiltersAndSorting();
        }

        private void ExportToCsv()
        {
            if (!ExportDialog.TryPickSavePath("magazyn.csv", out var filePath))
            {
                return;
            }

            try
            {
                var visibleWoods = _woodsView.Cast<Wood>().ToList();
                AppServices.Export.ExportWoods(visibleWoods, filePath);

                MessageBox.Show(
                    $"Wyeksportowano {visibleWoods.Count} pozycji do pliku:\n{filePath}",
                    "Eksport CSV",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eksport nie powiódł się:\n\n" + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportFromCsv()
        {
            if (!ExportDialog.TryPickOpenPath(out var filePath))
            {
                return;
            }

            try
            {
                var imported = AppServices.Import.ImportWoods(filePath, out var warnings);
                var mode = MessageBox.Show(
                    $"Wczytano {imported.Count} pozycji z pliku.\n\n" +
                    "Tak — scal z magazynem (dopasowane pozycje zwiększą ilość)\n" +
                    "Nie — zastąp cały magazyn\n" +
                    "Anuluj — przerwij",
                    "Import CSV",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (mode == MessageBoxResult.Cancel)
                {
                    return;
                }

                if (mode == MessageBoxResult.Yes)
                {
                    var result = AppServices.Inventory.MergeImportedWoods(_woods, imported);
                    LoadData();

                    var message = $"Dodano: {result.Added}, scalono: {result.Merged}.";
                    if (warnings.Count > 0)
                    {
                        message += $"\n\nPominięte wiersze ({warnings.Count}):\n" + string.Join("\n", warnings.Take(5));
                        if (warnings.Count > 5)
                        {
                            message += $"\n... i {warnings.Count - 5} więcej.";
                        }
                    }

                    MessageBox.Show(message, "Import CSV", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    AppServices.Inventory.ReplaceWoods(_woods, imported);
                    LoadData();
                    MessageBox.Show(
                        $"Magazyn został zastąpiony ({imported.Count} pozycji).",
                        "Import CSV",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Import nie powiódł się:\n\n" + ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string GetComboText(ComboBox comboBox, string fallback = "")
        {
            return ((ComboBoxItem)comboBox.SelectedItem)?.Content?.ToString() ?? fallback;
        }
    }
}
