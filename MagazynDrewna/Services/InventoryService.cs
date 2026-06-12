using System;
using System.Collections.Generic;
using System.Linq;
using MagazynDrewna.Data;
using MagazynDrewna.Models;

namespace MagazynDrewna.Services
{
    internal class ImportResult
    {
        public int Added { get; set; }
        public int Merged { get; set; }
        public int Skipped { get; set; }
    }

    internal class InventoryService
    {
        private readonly SQLiteBaza _storage;

        public InventoryService(SQLiteBaza storage)
        {
            _storage = storage;
        }

        public void Initialize()
        {
            _storage.Initialize();
        }

        public List<Wood> LoadWoods(bool seedIfEmpty = true)
        {
            var woods = _storage.LoadAllWoods();
            if (woods.Count == 0 && seedIfEmpty)
            {
                woods = CreateDefaultWoods();
                _storage.SaveAllWoods(woods);
            }

            return woods;
        }

        public void SaveWoods(List<Wood> woods)
        {
            _storage.SaveAllWoods(woods);
        }

        public ImportResult MergeImportedWoods(List<Wood> woods, IEnumerable<Wood> imported)
        {
            var result = new ImportResult();
            foreach (var item in imported)
            {
                if (string.IsNullOrWhiteSpace(item.Nazwa) || item.Dlugosc <= 0 || item.Ilosc < 0)
                {
                    result.Skipped++;
                    continue;
                }

                var existing = FindMatchingWood(woods, item);
                if (existing != null)
                {
                    existing.Ilosc += item.Ilosc;
                    result.Merged++;
                }
                else
                {
                    woods.Add(new Wood
                    {
                        Id = _storage.GetNextWoodId(woods),
                        Nazwa = item.Nazwa,
                        Gatunek = item.Gatunek,
                        Dlugosc = item.Dlugosc,
                        Ilosc = item.Ilosc,
                        Lokalizacja = item.Lokalizacja
                    });
                    result.Added++;
                }
            }

            _storage.SaveAllWoods(woods);
            return result;
        }

        public void ReplaceWoods(List<Wood> woods, IEnumerable<Wood> imported)
        {
            woods.Clear();
            var nextId = 1;
            foreach (var item in imported)
            {
                woods.Add(new Wood
                {
                    Id = nextId++,
                    Nazwa = item.Nazwa,
                    Gatunek = item.Gatunek,
                    Dlugosc = item.Dlugosc,
                    Ilosc = item.Ilosc,
                    Lokalizacja = item.Lokalizacja
                });
            }

            _storage.SaveAllWoods(woods);
        }

        public void ApplyDeliveryToInventory(List<Wood> woods, IEnumerable<PozycjaDostawy> pozycje)
        {
            foreach (var pozycja in pozycje)
            {
                var existing = FindMatchingWood(woods, pozycja);

                if (existing != null)
                {
                    existing.Ilosc += pozycja.Ilosc;
                }
                else
                {
                    woods.Add(new Wood
                    {
                        Id = _storage.GetNextWoodId(woods),
                        Nazwa = pozycja.Nazwa,
                        Gatunek = pozycja.Gatunek,
                        Dlugosc = pozycja.Dlugosc,
                        Ilosc = pozycja.Ilosc,
                        Lokalizacja = pozycja.Lokalizacja
                    });
                }
            }
        }

        public void ReverseDeliveryFromInventory(List<Wood> woods, IEnumerable<PozycjaDostawy> pozycje)
        {
            foreach (var pozycja in pozycje)
            {
                var existing = FindMatchingWood(woods, pozycja);
                if (existing == null)
                {
                    throw new InvalidOperationException(
                        $"Nie można cofnąć pozycji „{pozycja.Nazwa}” — brak pasującego towaru w magazynie.");
                }

                if (existing.Ilosc < pozycja.Ilosc)
                {
                    throw new InvalidOperationException(
                        $"Nie można cofnąć pozycji „{pozycja.Nazwa}” — w magazynie jest tylko {existing.Ilosc} szt., a dostawa miała {pozycja.Ilosc} szt.");
                }

                existing.Ilosc -= pozycja.Ilosc;
                if (existing.Ilosc == 0)
                {
                    woods.Remove(existing);
                }
            }
        }

        private static Wood FindMatchingWood(List<Wood> woods, PozycjaDostawy pozycja)
        {
            return woods.FirstOrDefault(w => MatchesWood(w, pozycja.Nazwa, pozycja.Gatunek, pozycja.Lokalizacja, pozycja.Dlugosc));
        }

        private static Wood FindMatchingWood(List<Wood> woods, Wood item)
        {
            return woods.FirstOrDefault(w => MatchesWood(w, item.Nazwa, item.Gatunek, item.Lokalizacja, item.Dlugosc));
        }

        private static bool MatchesWood(Wood wood, string nazwa, string gatunek, string lokalizacja, double dlugosc)
        {
            return string.Equals(wood.Nazwa, nazwa, StringComparison.CurrentCultureIgnoreCase) &&
                string.Equals(wood.Gatunek, gatunek, StringComparison.CurrentCultureIgnoreCase) &&
                string.Equals(wood.Lokalizacja, lokalizacja, StringComparison.CurrentCultureIgnoreCase) &&
                Math.Abs(wood.Dlugosc - dlugosc) < 0.0001;
        }

        private static List<Wood> CreateDefaultWoods()
        {
            return new List<Wood>
            {
                new Wood { Id = 1, Nazwa = "Sosna", Gatunek = "Iglaste", Dlugosc = 2.5, Ilosc = 30, Lokalizacja = "Magazyn A" },
                new Wood { Id = 2, Nazwa = "Dąb", Gatunek = "Liściaste", Dlugosc = 3.0, Ilosc = 15, Lokalizacja = "Magazyn B" },
                new Wood { Id = 3, Nazwa = "Świerk", Gatunek = "Iglaste", Dlugosc = 2.0, Ilosc = 50, Lokalizacja = "Magazyn C" },
                new Wood { Id = 4, Nazwa = "Buk", Gatunek = "Liściaste", Dlugosc = 3.0, Ilosc = 20, Lokalizacja = "Magazyn D" }
            };
        }
    }
}
