using System;
using System.Collections.Generic;
using System.Linq;

namespace MagazynDrewna.Models
{
    public class Dostawa
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Dostawca { get; set; }
        public string NumerDokumentu { get; set; }
        public string Uwagi { get; set; }
        public List<PozycjaDostawy> Pozycje { get; set; } = new List<PozycjaDostawy>();

        
        public bool Zrealizowana { get; set; }

        public string Status
        {
            get
            {
                if (Zrealizowana)
                {
                    return "Zrealizowana";
                }

                return Data.Date > DateTime.Today ? "Zaplanowana" : "Oczekuje";
            }
        }

        public bool MoznaPrzyjacDoMagazynu => !Zrealizowana && Data.Date <= DateTime.Today;

        public int LacznaIlosc => Pozycje.Count > 0 ? Pozycje.Sum(p => p.Ilosc) : 0;

        public string OpisPozycji => Pozycje.Count > 0
            ? string.Join(", ", Pozycje.Select(p => $"{p.Nazwa} ({p.Ilosc} szt.)"))
            : "—";
    }
}
