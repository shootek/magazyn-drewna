using MagazynDrewna.Data;
using MagazynDrewna.Services;

namespace MagazynDrewna
{
    internal static class AppServices
    {
        private static readonly SQLiteBaza Storage = new SQLiteBaza();

        public static InventoryService Inventory { get; } = new InventoryService(Storage);
        public static DeliveryService Delivery { get; } = new DeliveryService(Storage, Inventory);
        public static ReportService Reports { get; } = new ReportService();
        public static CsvExportService Export { get; } = new CsvExportService();
        public static CsvImportService Import { get; } = new CsvImportService();
    }
}
