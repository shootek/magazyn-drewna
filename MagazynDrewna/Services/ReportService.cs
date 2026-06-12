using System;
using System.Collections.Generic;
using System.Linq;
using MagazynDrewna.Models;

namespace MagazynDrewna.Services
{
    public class GrupowanieStatystyki
    {
        public string Nazwa { get; set; }
        public int IloscSztuk { get; set; }
        public double MetryBiezace { get; set; }
    }

    public class PodsumowanieMagazynu
    {
        public int LacznaIloscSztuk { get; set; }
        public double LaczneMetryBiezace { get; set; }
        public int LiczbaPozycji { get; set; }
        public int LiczbaGatunkow { get; set; }
        public List<GrupowanieStatystyki> WedlugGatunku { get; set; } = new List<GrupowanieStatystyki>();
        public List<GrupowanieStatystyki> WedlugLokalizacji { get; set; } = new List<GrupowanieStatystyki>();
        public List<GrupowanieStatystyki> WedlugNazwy { get; set; } = new List<GrupowanieStatystyki>();
        public List<Wood> NiskiStan { get; set; } = new List<Wood>();
    }

    public class PodsumowanieDostaw
    {
        public int LiczbaDostaw { get; set; }
        public int LacznaIloscSztuk { get; set; }
        public double LaczneMetryBiezace { get; set; }
        public List<GrupowanieStatystyki> WedlugDostawcy { get; set; } = new List<GrupowanieStatystyki>();
    }

    internal class ReportService
    {
        public PodsumowanieMagazynu BuildMagazynSummary(IEnumerable<Wood> woods, int niskiStanProg = 10)
        {
            var lista = woods.ToList();

            return new PodsumowanieMagazynu
            {
                LacznaIloscSztuk = lista.Sum(w => w.Ilosc),
                LaczneMetryBiezace = lista.Sum(w => w.MetryBiezace),
                LiczbaPozycji = lista.Count,
                LiczbaGatunkow = lista.Select(w => w.Nazwa).Distinct(StringComparer.CurrentCultureIgnoreCase).Count(),
                WedlugGatunku = GroupWoods(lista, w => w.Gatunek),
                WedlugLokalizacji = GroupWoods(lista, w => w.Lokalizacja),
                WedlugNazwy = GroupWoods(lista, w => w.Nazwa),
                NiskiStan = lista.Where(w => w.Ilosc <= niskiStanProg).OrderBy(w => w.Ilosc).ToList()
            };
        }

        public PodsumowanieDostaw BuildDeliverySummary(IEnumerable<Dostawa> deliveries, DateTime? od = null, DateTime? doDaty = null)
        {
            var filtered = deliveries.Where(d => d.Zrealizowana);

            if (od.HasValue)
            {
                filtered = filtered.Where(d => d.Data >= od.Value.Date);
            }

            if (doDaty.HasValue)
            {
                filtered = filtered.Where(d => d.Data <= doDaty.Value.Date);
            }

            var lista = filtered.ToList();
            var wszystkiePozycje = lista.SelectMany(d => d.Pozycje).ToList();

            return new PodsumowanieDostaw
            {
                LiczbaDostaw = lista.Count,
                LacznaIloscSztuk = wszystkiePozycje.Sum(p => p.Ilosc),
                LaczneMetryBiezace = wszystkiePozycje.Sum(p => p.MetryBiezace),
                WedlugDostawcy = lista
                    .GroupBy(d => d.Dostawca ?? "—", StringComparer.CurrentCultureIgnoreCase)
                    .Select(g => new GrupowanieStatystyki
                    {
                        Nazwa = g.Key,
                        IloscSztuk = g.SelectMany(d => d.Pozycje).Sum(p => p.Ilosc),
                        MetryBiezace = g.SelectMany(d => d.Pozycje).Sum(p => p.MetryBiezace)
                    })
                    .OrderByDescending(g => g.IloscSztuk)
                    .ToList()
            };
        }

        private static List<GrupowanieStatystyki> GroupWoods(List<Wood> woods, Func<Wood, string> keySelector)
        {
            return woods
                .GroupBy(keySelector, StringComparer.CurrentCultureIgnoreCase)
                .Select(g => new GrupowanieStatystyki
                {
                    Nazwa = g.Key,
                    IloscSztuk = g.Sum(w => w.Ilosc),
                    MetryBiezace = g.Sum(w => w.MetryBiezace)
                })
                .OrderByDescending(g => g.IloscSztuk)
                .ToList();
        }
    }
}
