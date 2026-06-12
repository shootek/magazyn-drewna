using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MagazynDrewna.Models;

namespace MagazynDrewna.Services
{
    internal class CsvImportService
    {
        public List<Wood> ImportWoods(string filePath, out List<string> warnings)
        {
            warnings = new List<string>();
            var lines = ReadAllLines(filePath);
            if (lines.Count == 0)
            {
                throw new InvalidOperationException("Plik CSV jest pusty.");
            }

            var dataLines = SkipPreamble(lines, out var headerColumns);
            if (headerColumns == null || headerColumns.Length < 6)
            {
                throw new InvalidOperationException(
                    "Nie rozpoznano nagłówka. Oczekiwane kolumny: Id;Nazwa;Gatunek;Dlugosc;Ilosc;Lokalizacja.");
            }

            var columnMap = MapColumns(headerColumns);
            var result = new List<Wood>();
            var lineNumber = 0;

            foreach (var line in dataLines)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var cells = ParseLine(line);
                if (cells.Length == 0)
                {
                    continue;
                }

                try
                {
                    var wood = ParseWoodRow(cells, columnMap);
                    result.Add(wood);
                }
                catch (Exception ex)
                {
                    warnings.Add($"Wiersz {lineNumber}: {ex.Message}");
                }
            }

            if (result.Count == 0)
            {
                throw new InvalidOperationException("Nie znaleziono poprawnych pozycji magazynu w pliku.");
            }

            return result;
        }

        private static Wood ParseWoodRow(string[] cells, Dictionary<string, int> columnMap)
        {
            var nazwa = GetCell(cells, columnMap, "Nazwa");
            var gatunek = GetCell(cells, columnMap, "Gatunek");
            var lokalizacja = GetCell(cells, columnMap, "Lokalizacja");

            if (string.IsNullOrWhiteSpace(nazwa))
            {
                throw new InvalidOperationException("brak nazwy.");
            }

            if (string.IsNullOrWhiteSpace(gatunek))
            {
                throw new InvalidOperationException("brak gatunku.");
            }

            if (string.IsNullOrWhiteSpace(lokalizacja))
            {
                throw new InvalidOperationException("brak lokalizacji.");
            }

            var dlugoscText = GetCell(cells, columnMap, "Dlugosc");
            var iloscText = GetCell(cells, columnMap, "Ilosc");

            if (!TryParseDouble(dlugoscText, out var dlugosc) || dlugosc <= 0)
            {
                throw new InvalidOperationException("niepoprawna długość.");
            }

            if (!TryParseInt(iloscText, out var ilosc) || ilosc < 0)
            {
                throw new InvalidOperationException("niepoprawna ilość.");
            }

            var idText = GetCell(cells, columnMap, "Id");
            int id = 0;
            if (!string.IsNullOrWhiteSpace(idText))
            {
                TryParseInt(idText, out id);
            }

            return new Wood
            {
                Id = id,
                Nazwa = nazwa.Trim(),
                Gatunek = gatunek.Trim(),
                Dlugosc = dlugosc,
                Ilosc = ilosc,
                Lokalizacja = lokalizacja.Trim()
            };
        }

        private static string GetCell(string[] cells, Dictionary<string, int> columnMap, string name)
        {
            if (!columnMap.TryGetValue(name, out var index) || index >= cells.Length)
            {
                return string.Empty;
            }

            return cells[index] ?? string.Empty;
        }

        private static Dictionary<string, int> MapColumns(string[] headerColumns)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headerColumns.Length; i++)
            {
                var name = headerColumns[i].Trim();
                if (!string.IsNullOrEmpty(name) && !map.ContainsKey(name))
                {
                    map[name] = i;
                }
            }

            if (!map.ContainsKey("Nazwa"))
            {
                return null;
            }

            return map;
        }

        private static List<string> SkipPreamble(List<string> lines, out string[] headerColumns)
        {
            headerColumns = null;
            var dataStart = 0;

            for (var i = 0; i < lines.Count; i++)
            {
                var trimmed = lines[i].Trim();
                if (trimmed.StartsWith("sep=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(trimmed))
                {
                    continue;
                }

                var columns = ParseLine(trimmed);
                if (IsWoodHeader(columns))
                {
                    headerColumns = columns;
                    dataStart = i + 1;
                    break;
                }

                if (headerColumns == null && columns.Length >= 4)
                {
                    headerColumns = columns;
                    dataStart = i + 1;
                    break;
                }
            }

            if (headerColumns == null)
            {
                return new List<string>();
            }

            return lines.Skip(dataStart).ToList();
        }

        private static bool IsWoodHeader(string[] columns)
        {
            if (columns.Length < 4)
            {
                return false;
            }

            return columns.Any(c => string.Equals(c.Trim(), "Nazwa", StringComparison.OrdinalIgnoreCase))
                && columns.Any(c => string.Equals(c.Trim(), "Gatunek", StringComparison.OrdinalIgnoreCase));
        }

        private static List<string> ReadAllLines(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                return File.ReadAllLines(filePath, Encoding.UTF8).ToList();
            }

            try
            {
                return File.ReadAllLines(filePath, Encoding.GetEncoding(1250)).ToList();
            }
            catch
            {
                return File.ReadAllLines(filePath, Encoding.UTF8).ToList();
            }
        }

        private static string[] ParseLine(string line)
        {
            var cells = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if (ch == ';' && !inQuotes)
                {
                    cells.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(ch);
            }

            cells.Add(current.ToString());
            return cells.ToArray();
        }

        private static bool TryParseDouble(string text, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var normalized = text.Trim().Replace(',', '.');
            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                || double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out value);
        }

        private static bool TryParseInt(string text, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            return int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.CurrentCulture, out value)
                || int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }
    }
}
