using System;
using System.Collections.Generic;
using System.Linq;
using MagazynDrewna.Data;
using MagazynDrewna.Models;

namespace MagazynDrewna.Services
{
    internal class DeliveryService
    {
        private readonly SQLiteBaza _storage;
        private readonly InventoryService _inventoryService;

        public DeliveryService(SQLiteBaza storage, InventoryService inventoryService)
        {
            _storage = storage;
            _inventoryService = inventoryService;
        }

        public List<Dostawa> LoadDeliveries()
        {
            return _storage.LoadAllDeliveries();
        }

        public void RegisterDelivery(Dostawa dostawa, List<Wood> woods)
        {
            dostawa.Id = _storage.GetNextDeliveryId();

            if (dostawa.Data.Date <= DateTime.Today)
            {
                dostawa.Zrealizowana = true;
                _inventoryService.ApplyDeliveryToInventory(woods, dostawa.Pozycje);
                _storage.SaveAllWoods(woods);
            }
            else
            {
                dostawa.Zrealizowana = false;
            }

            _storage.SaveDelivery(dostawa);
        }

        public void CompletePlannedDelivery(Dostawa dostawa, List<Wood> woods)
        {
            if (dostawa == null)
            {
                throw new InvalidOperationException("Nie wybrano dostawy.");
            }

            if (dostawa.Zrealizowana)
            {
                throw new InvalidOperationException("Ta dostawa została już przyjęta do magazynu.");
            }

            if (dostawa.Data.Date > DateTime.Today)
            {
                throw new InvalidOperationException(
                    $"Przyjęcie możliwe dopiero w dniu dostawy ({dostawa.Data:d}).");
            }

            _inventoryService.ApplyDeliveryToInventory(woods, dostawa.Pozycje);
            _storage.SaveAllWoods(woods);
            _storage.SetDeliveryCompleted(dostawa.Id);
            dostawa.Zrealizowana = true;
        }

        public void UpdateDelivery(Dostawa original, Dostawa updated, List<Wood> woods)
        {
            if (original == null)
            {
                throw new InvalidOperationException("Nie wybrano dostawy do edycji.");
            }

            if (updated == null || updated.Id != original.Id)
            {
                throw new InvalidOperationException("Nieprawidłowe dane edytowanej dostawy.");
            }

            if (original.Zrealizowana)
            {
                throw new InvalidOperationException("Zrealizowanej dostawy nie można edytować.");
            }

            if (updated.Data.Date <= DateTime.Today)
            {
                updated.Zrealizowana = true;
                _inventoryService.ApplyDeliveryToInventory(woods, updated.Pozycje);
                _storage.SaveAllWoods(woods);
            }
            else
            {
                updated.Zrealizowana = false;
            }

            _storage.UpdateDelivery(updated);
            original.Data = updated.Data;
            original.Dostawca = updated.Dostawca;
            original.NumerDokumentu = updated.NumerDokumentu;
            original.Uwagi = updated.Uwagi;
            original.Zrealizowana = updated.Zrealizowana;
            original.Pozycje = updated.Pozycje;
        }
    }
}
