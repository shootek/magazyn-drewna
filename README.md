# magazyn-drewna

Magazyn Drewna

Aplikacja desktopowa stworzona w technologii WPF służąca do zarządzania stanem magazynu drewna. Program umożliwia wygodne przeglądanie listy dostępnych pozycji magazynowych oraz wykonywanie podstawowych operacji CRUD (Create, Read, Update, Delete).
Użytkownik może dodawać nowe rekordy zawierające informacje o drewnie (np. gatunek, ilość, wymiary), edytować istniejące dane oraz usuwać wybrane elementy z dodatkowym potwierdzeniem, co zapobiega przypadkowej utracie danych. Aplikacja zawiera również mechanizmy walidacji danych wejściowych, które zapewniają poprawność i spójność wprowadzanych informacji.

Glowne funkcje aplikacji
- Lista elementow magazynowych w tabeli (`DataGrid`).
- Formularz szczegolow z dwoma trybami: podglad i edycja.
- Dodawanie nowego elementu (`Nowy` -> `OK` / `Anuluj`).
- Edycja wybranego elementu (`Edytuj` -> `OK` / `Anuluj`).
- Usuwanie elementu z oknem potwierdzenia.
- Walidacja danych (nazwa, liczby, lokalizacja, gatunek) i komunikaty bledow.

Postęp prac
- Krok 1 (1. zjazd) - Wybor tematu i zakresu projektu.
- Krok 2 (3. zjazd) - Layout XAML, widok listy i formularza, dane testowe.
- Krok 3 (5. zjazd) - CRUD w pamieci, Data Binding, MVVM.
- Krok 4 (6. zjazd) - Rozbudowa interakcji, komendy, walidacja, obsluga bledow.
- Krok 5 (8. zjazd) - Trwałość danych, pełne MVP
- Krok 6 (9. zjazd) - Rozszerzenia i dopracowanie: dodatkowe funkcjonalności, poprawianie UI/UX, lepsza architektura, refaktoryzacja
- Krok 7 (10. zjazd) - demonstracja działania, omówienie architektury, ocena wkładu indywidualnego.

Jak uruchomic aplikacje
1. Otworz `MagazynDrewna.slnx` w Visual Studio.
2. Ustaw konfiguracje `Debug` i platforme `Any CPU`.
3. Uruchom projekt (`F5` lub `Ctrl+F5`).



