using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MagazynDrewna.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private Wood _selectedWood;
        private bool _isEditing;
        private bool _isAdding;
        private int _editingId;
        private string _editNazwa = string.Empty;
        private string _editGatunek = string.Empty;
        private string _editDlugosc = string.Empty;
        private string _editIlosc = string.Empty;
        private string _editLokalizacja = string.Empty;
        private string _errorMessage = string.Empty;

        public MainViewModel()
        {
            Woods = new ObservableCollection<Wood>
            {
                new Wood { Id = 1, Nazwa = "Sosna", Gatunek = "Iglaste", Dlugosc = 2.5, Ilosc = 30, Lokalizacja = "Magazyn A" },
                new Wood { Id = 2, Nazwa = "Dab", Gatunek = "Lisciaste", Dlugosc = 3.0, Ilosc = 15, Lokalizacja = "Magazyn B" },
                new Wood { Id = 3, Nazwa = "Swierk", Gatunek = "Iglaste", Dlugosc = 2.0, Ilosc = 50, Lokalizacja = "Magazyn C" },
                new Wood { Id = 4, Nazwa = "Buk", Gatunek = "Lisciaste", Dlugosc = 3.0, Ilosc = 20, Lokalizacja = "Magazyn D" }
            };

            BeginAddCommand = new RelayCommand(_ => BeginAdd(), _ => !IsEditing);
            BeginEditCommand = new RelayCommand(_ => BeginEdit(), _ => SelectedWood != null && !IsEditing);
            SaveCommand = new RelayCommand(_ => Save(), _ => IsEditing);
            CancelCommand = new RelayCommand(_ => Cancel(), _ => IsEditing);
            DeleteWoodCommand = new RelayCommand(_ => DeleteSelected(), _ => SelectedWood != null && !IsEditing);

            SelectedWood = Woods.FirstOrDefault();
        }

        public string[] Gatunki { get; } = { "Iglaste", "Lisciaste" };
        public string[] Lokalizacje { get; } = { "Magazyn A", "Magazyn B", "Magazyn C", "Magazyn D" };

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
                    OnPropertyChanged(nameof(HasSelection));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool HasSelection => SelectedWood != null;

        public bool IsEditing
        {
            get => _isEditing;
            private set
            {
                if (_isEditing == value) return;
                _isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPreviewMode));
                OnPropertyChanged(nameof(FormTitle));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsPreviewMode => !IsEditing;

        public string FormTitle => IsEditing ? "Formularz - tryb edycji" : "Formularz - tryb podgladu";

        public string EditNazwa
        {
            get => _editNazwa;
            set { _editNazwa = value; OnPropertyChanged(); }
        }

        public string EditGatunek
        {
            get => _editGatunek;
            set { _editGatunek = value; OnPropertyChanged(); }
        }

        public string EditDlugosc
        {
            get => _editDlugosc;
            set { _editDlugosc = value; OnPropertyChanged(); }
        }

        public string EditIlosc
        {
            get => _editIlosc;
            set { _editIlosc = value; OnPropertyChanged(); }
        }

        public string EditLokalizacja
        {
            get => _editLokalizacja;
            set { _editLokalizacja = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand BeginAddCommand { get; }

        public ICommand BeginEditCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand DeleteWoodCommand { get; }

        private int NextId()
        {
            return Woods.Count == 0 ? 1 : Woods.Max(w => w.Id) + 1;
        }

        private void BeginAdd()
        {
            _isAdding = true;
            _editingId = NextId();
            EditNazwa = string.Empty;
            EditGatunek = Gatunki[0];
            EditDlugosc = "0";
            EditIlosc = "0";
            EditLokalizacja = Lokalizacje[0];
            ErrorMessage = string.Empty;
            IsEditing = true;
        }

        private void BeginEdit()
        {
            if (SelectedWood == null)
            {
                ErrorMessage = "Wybierz element do edycji.";
                return;
            }

            _isAdding = false;
            _editingId = SelectedWood.Id;
            EditNazwa = SelectedWood.Nazwa;
            EditGatunek = SelectedWood.Gatunek;
            EditDlugosc = SelectedWood.Dlugosc.ToString(CultureInfo.CurrentCulture);
            EditIlosc = SelectedWood.Ilosc.ToString(CultureInfo.CurrentCulture);
            EditLokalizacja = SelectedWood.Lokalizacja;
            ErrorMessage = string.Empty;
            IsEditing = true;
        }

        private void Save()
        {
            if (!ValidateInputs(out var dlugosc, out var ilosc))
            {
                return;
            }

            try
            {
                if (_isAdding)
                {
                    var newWood = new Wood
                    {
                        Id = _editingId,
                        Nazwa = EditNazwa.Trim(),
                        Gatunek = EditGatunek,
                        Dlugosc = dlugosc,
                        Ilosc = ilosc,
                        Lokalizacja = EditLokalizacja.Trim()
                    };

                    Woods.Add(newWood);
                    SelectedWood = newWood;
                }
                else
                {
                    if (SelectedWood == null)
                    {
                        ErrorMessage = "Wybrany element juz nie istnieje.";
                        return;
                    }

                    SelectedWood.Nazwa = EditNazwa.Trim();
                    SelectedWood.Gatunek = EditGatunek;
                    SelectedWood.Dlugosc = dlugosc;
                    SelectedWood.Ilosc = ilosc;
                    SelectedWood.Lokalizacja = EditLokalizacja.Trim();
                }

                ErrorMessage = string.Empty;
                IsEditing = false;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Nie udalo sie zapisac zmian: " + ex.Message;
            }
        }

        private bool ValidateInputs(out double dlugosc, out int ilosc)
        {
            dlugosc = 0;
            ilosc = 0;

            if (string.IsNullOrWhiteSpace(EditNazwa))
            {
                ErrorMessage = "Nazwa jest wymagana.";
                return false;
            }

            if (EditNazwa.Any(char.IsDigit))
            {
                ErrorMessage = "Nazwa nie moze zawierac cyfr.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditGatunek))
            {
                ErrorMessage = "Wybierz gatunek.";
                return false;
            }

            if (!double.TryParse(EditDlugosc, NumberStyles.Float, CultureInfo.CurrentCulture, out dlugosc) || dlugosc <= 0)
            {
                ErrorMessage = "Dlugosc musi byc poprawna liczba wieksza od 0.";
                return false;
            }

            if (!int.TryParse(EditIlosc, NumberStyles.Integer, CultureInfo.CurrentCulture, out ilosc) || ilosc <= 0)
            {
                ErrorMessage = "Ilosc musi byc liczba calkowita > 0.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(EditLokalizacja))
            {
                ErrorMessage = "Lokalizacja jest wymagana.";
                return false;
            }

            if (!Lokalizacje.Contains(EditLokalizacja))
            {
                ErrorMessage = "Wybierz lokalizacje z listy.";
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        private void Cancel()
        {
            _isAdding = false;
            ErrorMessage = string.Empty;
            IsEditing = false;
        }

        private void DeleteSelected()
        {
            if (SelectedWood == null)
            {
                return;
            }

            var answer = MessageBox.Show(
                "Czy na pewno usunac wybrany element?",
                "Potwierdzenie usuniecia",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var index = Woods.IndexOf(SelectedWood);
                Woods.Remove(SelectedWood);
                SelectedWood = Woods.Count == 0 ? null : (index > 0 ? Woods[index - 1] : Woods[0]);
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Nie udalo sie usunac elementu: " + ex.Message;
            }
        }
    }
}
