using System.Collections.Generic;

namespace MagazynDrewna.Models
{
    public static class MagazynConstants
    {
        public static readonly IReadOnlyList<string> Gatunki = new[] { "Iglaste", "Liściaste" };

        public static readonly IReadOnlyList<string> Lokalizacje = new[]
        {
            "Magazyn A",
            "Magazyn B",
            "Magazyn C",
            "Magazyn D"
        };
    }
}
