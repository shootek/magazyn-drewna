# magazyn-drewna

Magazyn Drewna

Aplikacja desktopowa WPF do zarzadzania stanem magazynu drewna. Program pozwala przegladac liste pozycji, dodawac nowe rekordy, edytowac dane, usuwac elementy z potwierdzeniem oraz walidowac dane wejsciowe. Projekt zostal wykonany w architekturze MVVM (View + ViewModel + Model), z danymi przechowywanymi w pamieci.

Glowne funkcje aplikacji
- Lista elementow magazynowych w tabeli (`DataGrid`).
- Formularz szczegolow z dwoma trybami: podglad i edycja.
- Dodawanie nowego elementu (`Nowy` -> `OK` / `Anuluj`).
- Edycja wybranego elementu (`Edytuj` -> `OK` / `Anuluj`).
- Usuwanie elementu z oknem potwierdzenia.
- Walidacja danych (nazwa, liczby, lokalizacja, gatunek) i komunikaty bledow.

Podzial pracy
- Krok 1 (1. zjazd) - Wybor tematu i zakresu projektu.
- Krok 2 (3. zjazd) - Layout XAML, widok listy i formularza, dane testowe.
- Krok 3 (5. zjazd) - CRUD w pamieci, Data Binding, MVVM.
- Krok 4 (6. zjazd) - Rozbudowa interakcji, komendy, walidacja, obsluga bledow.
- Krok 4.5 (7. zjazd) - [opcjonalne konsultacje]
- Krok 5 (8. zjazd) - [do realizacji]
- Krok 6 (9. zjazd) - [do realizacji]
- Krok 7 (10. zjazd) - [do realizacji]


Struktura projektu + krotki opis zawartosci

.
├── MagazynDrewna/
├── |-- MagazynDrewna.slnx                 # Plik rozwiazania
├── |-- README.md                          # Dokumentacja projektu
├── |
└── `-- MagazynDrewna/
    ├── |-- App.xaml                       # Konfiguracja aplikacji WPF (startup)
    ├── |-- App.xaml.cs                    # Klasa App
    ├── |-- MainWindow.xaml                # Glowne okno UI (XAML)
    ├── |-- MainWindow.xaml.cs             # Inicjalizacja okna, ustawienie DataContext
    ├── |-- Wood.cs                        # Model danych drewna
    ├── |-- MagazynDrewna.csproj           # Konfiguracja projektu .NET Framework 4.7.2
    ├── |-- App.config                     # Konfiguracja aplikacji
    ├── |
    ├── |-- ViewModels/
    ├── |   |-- ViewModelBase.cs           # Bazowy INotifyPropertyChanged
    ├── |   |-- RelayCommand.cs            # Implementacja ICommand
    ├── |   `-- MainViewModel.cs           # Logika CRUD, tryby edycji, walidacja, obsluga bledow
    ├── |
    └── `-- Properties/
        ├── |-- AssemblyInfo.cs            # Metadane assembly
        ├── |-- Resources.resx             # Zasoby projektu
        ├── |-- Resources.Designer.cs      # Kod generowany dla zasobow
        ├── |-- Settings.settings          # Ustawienia aplikacji
        └── `-- Settings.Designer.cs       # Kod generowany dla ustawien
Jak uruchomic aplikacje
1. Otworz `MagazynDrewna.slnx` w Visual Studio.
2. Ustaw konfiguracje `Debug` i platforme `Any CPU`.
3. Uruchom projekt (`F5` lub `Ctrl+F5`).



