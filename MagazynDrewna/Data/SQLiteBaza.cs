using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Data.SQLite;
using MagazynDrewna.Models;

namespace MagazynDrewna.Data
{
    internal class SQLiteBaza
    {
        private readonly string _connectionString;

        public SQLiteBaza()
        {
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MagazynDrewna");
            var dbPath = Path.Combine(appData, "magazyn.db");
            _connectionString = $"Data Source={dbPath};Version=3;";
        }

        public void Initialize()
        {
            var dbPath = GetDbPath();
            var dir = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (var connection = OpenConnection())
            {
                ExecuteNonQuery(connection,
                    @"CREATE TABLE IF NOT EXISTS Woods (
                        Id INTEGER PRIMARY KEY,
                        Nazwa TEXT NOT NULL,
                        Gatunek TEXT NOT NULL,
                        Dlugosc REAL NOT NULL,
                        Ilosc INTEGER NOT NULL,
                        Lokalizacja TEXT NOT NULL
                    );");

                ExecuteNonQuery(connection,
                    @"CREATE TABLE IF NOT EXISTS Deliveries (
                        Id INTEGER PRIMARY KEY,
                        Data TEXT NOT NULL,
                        Dostawca TEXT NOT NULL,
                        NumerDokumentu TEXT,
                        Uwagi TEXT,
                        Zrealizowana INTEGER NOT NULL DEFAULT 1
                    );");

                EnsureDeliveryStatusColumn(connection);

                ExecuteNonQuery(connection,
                    @"CREATE TABLE IF NOT EXISTS DeliveryItems (
                        Id INTEGER PRIMARY KEY,
                        DostawaId INTEGER NOT NULL,
                        Nazwa TEXT NOT NULL,
                        Gatunek TEXT NOT NULL,
                        Dlugosc REAL NOT NULL,
                        Ilosc INTEGER NOT NULL,
                        Lokalizacja TEXT NOT NULL,
                        FOREIGN KEY (DostawaId) REFERENCES Deliveries(Id)
                    );");
            }
        }

        public List<Wood> LoadAllWoods()
        {
            var result = new List<Wood>();

            using (var connection = OpenConnection())
            using (var command = new SQLiteCommand(
                "SELECT Id, Nazwa, Gatunek, Dlugosc, Ilosc, Lokalizacja FROM Woods ORDER BY Id;", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(ReadWood(reader));
                }
            }

            return result;
        }

        public void SaveAllWoods(List<Wood> woods)
        {
            using (var connection = OpenConnection())
            using (var tx = connection.BeginTransaction())
            {
                ExecuteNonQuery(connection, "DELETE FROM Woods;", tx);

                foreach (var wood in woods)
                {
                    using (var insertCmd = new SQLiteCommand(
                        @"INSERT INTO Woods (Id, Nazwa, Gatunek, Dlugosc, Ilosc, Lokalizacja)
                          VALUES (@Id, @Nazwa, @Gatunek, @Dlugosc, @Ilosc, @Lokalizacja);",
                        connection, tx))
                    {
                        insertCmd.Parameters.AddWithValue("@Id", wood.Id);
                        insertCmd.Parameters.AddWithValue("@Nazwa", wood.Nazwa);
                        insertCmd.Parameters.AddWithValue("@Gatunek", wood.Gatunek);
                        insertCmd.Parameters.AddWithValue("@Dlugosc", wood.Dlugosc);
                        insertCmd.Parameters.AddWithValue("@Ilosc", wood.Ilosc);
                        insertCmd.Parameters.AddWithValue("@Lokalizacja", wood.Lokalizacja);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                tx.Commit();
            }
        }

        public List<Dostawa> LoadAllDeliveries()
        {
            var deliveries = new List<Dostawa>();

            using (var connection = OpenConnection())
            {
                using (var command = new SQLiteCommand(
                    "SELECT Id, Data, Dostawca, NumerDokumentu, Uwagi, Zrealizowana FROM Deliveries ORDER BY Data DESC, Id DESC;",
                    connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        deliveries.Add(ReadDelivery(reader));
                    }
                }

                foreach (var dostawa in deliveries)
                {
                    dostawa.Pozycje = LoadDeliveryItems(connection, dostawa.Id);
                }
            }

            return deliveries;
        }

        public void SaveDelivery(Dostawa dostawa)
        {
            using (var connection = OpenConnection())
            using (var tx = connection.BeginTransaction())
            {
                using (var insertCmd = new SQLiteCommand(
                    @"INSERT INTO Deliveries (Id, Data, Dostawca, NumerDokumentu, Uwagi, Zrealizowana)
                      VALUES (@Id, @Data, @Dostawca, @NumerDokumentu, @Uwagi, @Zrealizowana);",
                    connection, tx))
                {
                    insertCmd.Parameters.AddWithValue("@Id", dostawa.Id);
                    insertCmd.Parameters.AddWithValue("@Data", dostawa.Data.ToString("o", CultureInfo.InvariantCulture));
                    insertCmd.Parameters.AddWithValue("@Dostawca", dostawa.Dostawca);
                    insertCmd.Parameters.AddWithValue("@NumerDokumentu", (object)dostawa.NumerDokumentu ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@Uwagi", (object)dostawa.Uwagi ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@Zrealizowana", dostawa.Zrealizowana ? 1 : 0);
                    insertCmd.ExecuteNonQuery();
                }

                var nextItemId = GetNextId(connection, "DeliveryItems", tx);
                foreach (var pozycja in dostawa.Pozycje)
                {
                    pozycja.Id = nextItemId++;
                    pozycja.DostawaId = dostawa.Id;

                    using (var insertCmd = new SQLiteCommand(
                        @"INSERT INTO DeliveryItems (Id, DostawaId, Nazwa, Gatunek, Dlugosc, Ilosc, Lokalizacja)
                          VALUES (@Id, @DostawaId, @Nazwa, @Gatunek, @Dlugosc, @Ilosc, @Lokalizacja);",
                        connection, tx))
                    {
                        insertCmd.Parameters.AddWithValue("@Id", pozycja.Id);
                        insertCmd.Parameters.AddWithValue("@DostawaId", pozycja.DostawaId);
                        insertCmd.Parameters.AddWithValue("@Nazwa", pozycja.Nazwa);
                        insertCmd.Parameters.AddWithValue("@Gatunek", pozycja.Gatunek);
                        insertCmd.Parameters.AddWithValue("@Dlugosc", pozycja.Dlugosc);
                        insertCmd.Parameters.AddWithValue("@Ilosc", pozycja.Ilosc);
                        insertCmd.Parameters.AddWithValue("@Lokalizacja", pozycja.Lokalizacja);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                tx.Commit();
            }
        }

        public int GetNextWoodId(List<Wood> woods)
        {
            return woods.Count == 0 ? 1 : woods.Max(w => w.Id) + 1;
        }

        public int GetNextDeliveryId()
        {
            using (var connection = OpenConnection())
            {
                return GetNextId(connection, "Deliveries");
            }
        }

        public void SetDeliveryCompleted(int dostawaId)
        {
            using (var connection = OpenConnection())
            {
                using (var command = new SQLiteCommand(
                    "UPDATE Deliveries SET Zrealizowana = 1 WHERE Id = @Id;", connection))
                {
                    command.Parameters.AddWithValue("@Id", dostawaId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateDelivery(Dostawa dostawa)
        {
            using (var connection = OpenConnection())
            using (var tx = connection.BeginTransaction())
            {
                using (var updateCmd = new SQLiteCommand(
                    @"UPDATE Deliveries
                      SET Data = @Data, Dostawca = @Dostawca, NumerDokumentu = @NumerDokumentu,
                          Uwagi = @Uwagi, Zrealizowana = @Zrealizowana
                      WHERE Id = @Id;",
                    connection, tx))
                {
                    updateCmd.Parameters.AddWithValue("@Id", dostawa.Id);
                    updateCmd.Parameters.AddWithValue("@Data", dostawa.Data.ToString("o", CultureInfo.InvariantCulture));
                    updateCmd.Parameters.AddWithValue("@Dostawca", dostawa.Dostawca);
                    updateCmd.Parameters.AddWithValue("@NumerDokumentu", (object)dostawa.NumerDokumentu ?? DBNull.Value);
                    updateCmd.Parameters.AddWithValue("@Uwagi", (object)dostawa.Uwagi ?? DBNull.Value);
                    updateCmd.Parameters.AddWithValue("@Zrealizowana", dostawa.Zrealizowana ? 1 : 0);
                    updateCmd.ExecuteNonQuery();
                }

                using (var deleteCmd = new SQLiteCommand(
                    "DELETE FROM DeliveryItems WHERE DostawaId = @DostawaId;", connection, tx))
                {
                    deleteCmd.Parameters.AddWithValue("@DostawaId", dostawa.Id);
                    deleteCmd.ExecuteNonQuery();
                }

                var nextItemId = GetNextId(connection, "DeliveryItems", tx);
                foreach (var pozycja in dostawa.Pozycje)
                {
                    pozycja.Id = nextItemId++;
                    pozycja.DostawaId = dostawa.Id;

                    using (var insertCmd = new SQLiteCommand(
                        @"INSERT INTO DeliveryItems (Id, DostawaId, Nazwa, Gatunek, Dlugosc, Ilosc, Lokalizacja)
                          VALUES (@Id, @DostawaId, @Nazwa, @Gatunek, @Dlugosc, @Ilosc, @Lokalizacja);",
                        connection, tx))
                    {
                        insertCmd.Parameters.AddWithValue("@Id", pozycja.Id);
                        insertCmd.Parameters.AddWithValue("@DostawaId", pozycja.DostawaId);
                        insertCmd.Parameters.AddWithValue("@Nazwa", pozycja.Nazwa);
                        insertCmd.Parameters.AddWithValue("@Gatunek", pozycja.Gatunek);
                        insertCmd.Parameters.AddWithValue("@Dlugosc", pozycja.Dlugosc);
                        insertCmd.Parameters.AddWithValue("@Ilosc", pozycja.Ilosc);
                        insertCmd.Parameters.AddWithValue("@Lokalizacja", pozycja.Lokalizacja);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                tx.Commit();
            }
        }

        private string GetDbPath()
        {
            var builder = new SQLiteConnectionStringBuilder(_connectionString);
            return builder.DataSource;
        }

        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private static List<PozycjaDostawy> LoadDeliveryItems(SQLiteConnection connection, int dostawaId)
        {
            var items = new List<PozycjaDostawy>();

            using (var command = new SQLiteCommand(
                @"SELECT Id, DostawaId, Nazwa, Gatunek, Dlugosc, Ilosc, Lokalizacja
                  FROM DeliveryItems WHERE DostawaId = @DostawaId ORDER BY Id;",
                connection))
            {
                command.Parameters.AddWithValue("@DostawaId", dostawaId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        items.Add(new PozycjaDostawy
                        {
                            Id = Convert.ToInt32(reader["Id"], CultureInfo.InvariantCulture),
                            DostawaId = Convert.ToInt32(reader["DostawaId"], CultureInfo.InvariantCulture),
                            Nazwa = Convert.ToString(reader["Nazwa"]) ?? string.Empty,
                            Gatunek = Convert.ToString(reader["Gatunek"]) ?? string.Empty,
                            Dlugosc = Convert.ToDouble(reader["Dlugosc"], CultureInfo.InvariantCulture),
                            Ilosc = Convert.ToInt32(reader["Ilosc"], CultureInfo.InvariantCulture),
                            Lokalizacja = Convert.ToString(reader["Lokalizacja"]) ?? string.Empty
                        });
                    }
                }
            }

            return items;
        }

        private static Wood ReadWood(SQLiteDataReader reader)
        {
            return new Wood
            {
                Id = Convert.ToInt32(reader["Id"], CultureInfo.InvariantCulture),
                Nazwa = Convert.ToString(reader["Nazwa"]) ?? string.Empty,
                Gatunek = Convert.ToString(reader["Gatunek"]) ?? string.Empty,
                Dlugosc = Convert.ToDouble(reader["Dlugosc"], CultureInfo.InvariantCulture),
                Ilosc = Convert.ToInt32(reader["Ilosc"], CultureInfo.InvariantCulture),
                Lokalizacja = Convert.ToString(reader["Lokalizacja"]) ?? string.Empty
            };
        }

        private static Dostawa ReadDelivery(SQLiteDataReader reader)
        {
            var dataText = Convert.ToString(reader["Data"]) ?? string.Empty;
            DateTime data;
            if (!DateTime.TryParse(dataText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out data))
            {
                DateTime.TryParse(dataText, CultureInfo.CurrentCulture, DateTimeStyles.None, out data);
            }

            var dostawa = new Dostawa
            {
                Id = Convert.ToInt32(reader["Id"], CultureInfo.InvariantCulture),
                Data = data == default(DateTime) ? DateTime.Today : data.Date,
                Dostawca = Convert.ToString(reader["Dostawca"]) ?? string.Empty,
                NumerDokumentu = Convert.ToString(reader["NumerDokumentu"]) ?? string.Empty,
                Uwagi = Convert.ToString(reader["Uwagi"]) ?? string.Empty,
                Zrealizowana = ReadBoolColumn(reader, "Zrealizowana", defaultValue: true)
            };

            return dostawa;
        }

        private static void EnsureDeliveryStatusColumn(SQLiteConnection connection)
        {
            if (ColumnExists(connection, "Deliveries", "Zrealizowana"))
            {
                return;
            }

            ExecuteNonQuery(connection, "ALTER TABLE Deliveries ADD COLUMN Zrealizowana INTEGER NOT NULL DEFAULT 1;");
        }

        private static bool ColumnExists(SQLiteConnection connection, string table, string column)
        {
            using (var command = new SQLiteCommand($"PRAGMA table_info({table});", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (string.Equals(Convert.ToString(reader["name"]), column, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool ReadBoolColumn(SQLiteDataReader reader, string column, bool defaultValue)
        {
            try
            {
                var ordinal = reader.GetOrdinal(column);
                if (reader.IsDBNull(ordinal))
                {
                    return defaultValue;
                }

                return Convert.ToInt32(reader[ordinal], CultureInfo.InvariantCulture) != 0;
            }
            catch (IndexOutOfRangeException)
            {
                return defaultValue;
            }
        }

        private static int GetNextId(SQLiteConnection connection, string table, SQLiteTransaction tx = null)
        {
            using (var command = new SQLiteCommand($"SELECT COALESCE(MAX(Id), 0) + 1 FROM {table};", connection, tx))
            {
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        private static void ExecuteNonQuery(SQLiteConnection connection, string sql, SQLiteTransaction tx = null)
        {
            using (var command = new SQLiteCommand(sql, connection, tx))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
