using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Data.SQLite;

namespace MagazynDrewna
{
    internal class SQLiteBaza
    {
        private readonly string _dbPath;
        private readonly string _connectionString;

        public SQLiteBaza()
        {
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MagazynDrewna");
            _dbPath = Path.Combine(appData, "magazyn.db");
            _connectionString = $"Data Source={_dbPath};Version=3;";
        }

        public void Initialize()
        {
            var dir = Path.GetDirectoryName(_dbPath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(_dbPath))
            {
                SQLiteConnection.CreateFile(_dbPath);
            }

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(
                    @"CREATE TABLE IF NOT EXISTS Woods (
                        Id INTEGER PRIMARY KEY,
                        Nazwa TEXT NOT NULL,
                        Gatunek TEXT NOT NULL,
                        Dlugosc REAL NOT NULL,
                        Ilosc INTEGER NOT NULL,
                        Lokalizacja TEXT NOT NULL
                    );", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Wood> LoadAll()
        {
            var result = new List<Wood>();

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(
                    "SELECT Id, Nazwa, Gatunek, Dlugosc, Ilosc, Lokalizacja FROM Woods ORDER BY Id;", connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new Wood
                        {
                            Id = Convert.ToInt32(reader["Id"], CultureInfo.InvariantCulture),
                            Nazwa = Convert.ToString(reader["Nazwa"]) ?? string.Empty,
                            Gatunek = Convert.ToString(reader["Gatunek"]) ?? string.Empty,
                            Dlugosc = Convert.ToDouble(reader["Dlugosc"], CultureInfo.InvariantCulture),
                            Ilosc = Convert.ToInt32(reader["Ilosc"], CultureInfo.InvariantCulture),
                            Lokalizacja = Convert.ToString(reader["Lokalizacja"]) ?? string.Empty
                        });
                    }
                }
            }

            return result;
        }

        public void SaveAll(List<Wood> woods)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var tx = connection.BeginTransaction())
                {
                    using (var deleteCmd = new SQLiteCommand("DELETE FROM Woods;", connection, tx))
                    {
                        deleteCmd.ExecuteNonQuery();
                    }

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
        }
    }
}
