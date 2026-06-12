using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MagazynDrewna.Models;

namespace MagazynDrewna.Services
{
    internal class CsvExportService
    {
        private const char Delimiter = ';';

        public void ExportWoods(IEnumerable<Wood> woods, string filePath)
        {
            var lines = new List<string> { "sep=;" };
            lines.Add(JoinRow("Id", "Nazwa", "Gatunek", "Dlugosc", "Ilosc", "Lokalizacja", "MetryBiezace"));

            foreach (var wood in woods)
            {
                lines.Add(JoinRow(
                    wood.Id,
                    Escape(wood.Nazwa),
                    Escape(wood.Gatunek),
                    FormatNumber(wood.Dlugosc),
                    wood.Ilosc,
                    Escape(wood.Lokalizacja),
                    FormatNumber(wood.MetryBiezace)));
            }

            WriteCsv(filePath, lines);
        }

        public void ExportSummary(PodsumowanieMagazynu magazyn, PodsumowanieDostaw dostawy, string filePath)
        {
            var lines = new List<string>
            {
                "sep=;",
                JoinRow("Raport", "Magazyn Drewna"),
                JoinRow("Wygenerowano", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.CurrentCulture)),
                string.Empty,
                "PODSUMOWANIE MAGAZYNU",
                JoinRow("Metryka", "Wartość"),
                JoinRow("Łączna ilość sztuk", magazyn.LacznaIloscSztuk),
                JoinRow("Metry bieżące", FormatNumber(magazyn.LaczneMetryBiezace)),
                JoinRow("Liczba pozycji", magazyn.LiczbaPozycji),
                JoinRow("Liczba gatunków", magazyn.LiczbaGatunkow),
                string.Empty,
                "WEDŁUG GATUNKU",
                JoinRow("Gatunek", "Sztuki", "MetryBiezace")
            };

            lines.AddRange(magazyn.WedlugGatunku.Select(g =>
                JoinRow(Escape(g.Nazwa), g.IloscSztuk, FormatNumber(g.MetryBiezace))));

            lines.Add(string.Empty);
            lines.Add("WEDŁUG LOKALIZACJI");
            lines.Add(JoinRow("Lokalizacja", "Sztuki", "MetryBiezace"));
            lines.AddRange(magazyn.WedlugLokalizacji.Select(g =>
                JoinRow(Escape(g.Nazwa), g.IloscSztuk, FormatNumber(g.MetryBiezace))));

            lines.Add(string.Empty);
            lines.Add("WEDŁUG NAZWY");
            lines.Add(JoinRow("Nazwa", "Sztuki", "MetryBiezace"));
            lines.AddRange(magazyn.WedlugNazwy.Select(g =>
                JoinRow(Escape(g.Nazwa), g.IloscSztuk, FormatNumber(g.MetryBiezace))));

            lines.Add(string.Empty);
            lines.Add("NISKI STAN (<= 10 szt.)");
            lines.Add(JoinRow("Nazwa", "Gatunek", "Ilosc", "Lokalizacja"));
            lines.AddRange(magazyn.NiskiStan.Select(w =>
                JoinRow(Escape(w.Nazwa), Escape(w.Gatunek), w.Ilosc, Escape(w.Lokalizacja))));

            lines.Add(string.Empty);
            lines.Add("PODSUMOWANIE DOSTAW");
            lines.Add(JoinRow("Metryka", "Wartość"));
            lines.Add(JoinRow("Liczba dostaw", dostawy.LiczbaDostaw));
            lines.Add(JoinRow("Przyjęte sztuki", dostawy.LacznaIloscSztuk));
            lines.Add(JoinRow("Przyjęte mb", FormatNumber(dostawy.LaczneMetryBiezace)));

            if (dostawy.WedlugDostawcy.Any())
            {
                lines.Add(string.Empty);
                lines.Add("WEDŁUG DOSTAWCY");
                lines.Add(JoinRow("Dostawca", "Sztuki", "MetryBiezace"));
                lines.AddRange(dostawy.WedlugDostawcy.Select(g =>
                    JoinRow(Escape(g.Nazwa), g.IloscSztuk, FormatNumber(g.MetryBiezace))));
            }

            WriteCsv(filePath, lines);
        }

        private static string JoinRow(params object[] values)
        {
            return string.Join(Delimiter.ToString(), values);
        }

        private static string FormatNumber(double value)
        {
            return value.ToString(CultureInfo.CurrentCulture);
        }

        private static void WriteCsv(string filePath, IEnumerable<string> lines)
        {
            
            var encoding = Encoding.GetEncoding(1250);
            File.WriteAllText(filePath, string.Join("\r\n", lines), encoding);
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.Contains(",") || value.Contains(";") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }
    }
}
