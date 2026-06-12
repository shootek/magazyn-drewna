namespace MagazynDrewna.Models
{
    public class PozycjaDostawy
    {
        public int Id { get; set; }
        public int DostawaId { get; set; }
        public string Nazwa { get; set; }
        public string Gatunek { get; set; }
        public double Dlugosc { get; set; }
        public int Ilosc { get; set; }
        public string Lokalizacja { get; set; }

        public double MetryBiezace => Dlugosc * Ilosc;
    }
}
