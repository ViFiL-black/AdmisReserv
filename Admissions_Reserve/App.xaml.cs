// App.xaml.cs
using System;
using System.IO;
using System.Windows;
using Admissions_Reserve.Model;

namespace Admissions_Reserve
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Принудительно создаем базу данных при запуске
                InitializeDatabase();

                // Показываем путь к БД (для отладки)
                string dbPath = DatabaseHelper.GetDatabasePath();
                if (File.Exists(dbPath))
                {
                    MessageBox.Show($"База данных создана успешно!\nПуть: {dbPath}",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}\n\n{ex.StackTrace}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeDatabase()
        {
            // Создаем директорию для БД если её нет
            string dbDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }

            string dbPath = Path.Combine(dbDirectory, "AdmissionsReserve.db");

            // Проверяем, существует ли файл БД
            if (!File.Exists(dbPath))
            {
                // Создаем пустой файл БД
                System.Data.SQLite.SQLiteConnection.CreateFile(dbPath);

                // Создаем таблицы и заполняем данными
                CreateTablesAndSeedData(dbPath);
            }
        }

        private void CreateTablesAndSeedData(string dbPath)
        {
            string connectionString = $"Data Source={dbPath};Version=3;Foreign Keys=True;";

            using (var connection = new System.Data.SQLite.SQLiteConnection(connectionString))
            {
                connection.Open();

                // Включаем поддержку внешних ключей
                using (var cmd = new System.Data.SQLite.SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // Создаем таблицы
                var createTableCommands = new[]
                {
                    @"CREATE TABLE IF NOT EXISTS Countries (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        IsActive INTEGER DEFAULT 1
                    )",

                    @"CREATE TABLE IF NOT EXISTS Genders (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL
                    )",

                    @"CREATE TABLE IF NOT EXISTS Citizenships (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL
                    )",

                    @"CREATE TABLE IF NOT EXISTS IdentityDocumentTypes (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL
                    )",

                    @"CREATE TABLE IF NOT EXISTS Applicants (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        LastName TEXT,
                        FirstName TEXT,
                        Patronymic TEXT,
                        BirthDate TEXT,
                        BirthPlace TEXT,
                        GenderId INTEGER,
                        CitizenshipId INTEGER,
                        Snils TEXT,
                        RegistrationCountryId INTEGER,
                        RegistrationPostalCode TEXT,
                        RegistrationRegion TEXT,
                        RegistrationDistrict TEXT,
                        RegistrationCity TEXT,
                        RegistrationStreet TEXT,
                        RegistrationHouse TEXT,
                        RegistrationBuilding TEXT,
                        RegistrationApartment TEXT,
                        ActualAddressSame INTEGER,
                        ActualCountryId INTEGER,
                        ActualPostalCode TEXT,
                        ActualRegion TEXT,
                        ActualDistrict TEXT,
                        ActualCity TEXT,
                        ActualStreet TEXT,
                        ActualHouse TEXT,
                        ActualBuilding TEXT,
                        ActualApartment TEXT,
                        Phone TEXT,
                        MobilePhone TEXT,
                        Fax TEXT,
                        WorkPhone TEXT,
                        Email TEXT,
                        AdditionalEmail TEXT,
                        Website TEXT,
                        Telegram TEXT,
                        WhatsApp TEXT,
                        Viber TEXT,
                        PreferredContactMethod TEXT,
                        ContactComment TEXT,
                        CreatedAt TEXT NOT NULL,
                        UpdatedAt TEXT NOT NULL,
                        FOREIGN KEY (GenderId) REFERENCES Genders(Id),
                        FOREIGN KEY (CitizenshipId) REFERENCES Citizenships(Id),
                        FOREIGN KEY (RegistrationCountryId) REFERENCES Countries(Id),
                        FOREIGN KEY (ActualCountryId) REFERENCES Countries(Id)
                    )",

                    @"CREATE TABLE IF NOT EXISTS IdentityDocuments (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ApplicantId INTEGER NOT NULL,
                        DocumentTypeId INTEGER,
                        Series TEXT,
                        Number TEXT,
                        IssuedBy TEXT,
                        IssueDate TEXT,
                        DepartmentCode TEXT,
                        IsPrimary INTEGER,
                        AddedDate TEXT NOT NULL,
                        FOREIGN KEY (ApplicantId) REFERENCES Applicants(Id) ON DELETE CASCADE,
                        FOREIGN KEY (DocumentTypeId) REFERENCES IdentityDocumentTypes(Id)
                    )",

                    @"CREATE TABLE IF NOT EXISTS ChangeHistory (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        TableName TEXT,
                        RecordId INTEGER,
                        Action TEXT,
                        ChangedAt TEXT NOT NULL
                    )"
                };

                foreach (var commandText in createTableCommands)
                {
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(commandText, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // Заполняем справочные данные
                SeedReferenceData(connection);
            }
        }

        private void SeedReferenceData(System.Data.SQLite.SQLiteConnection connection)
        {
            // Проверяем, есть ли уже данные
            using (var cmd = new System.Data.SQLite.SQLiteCommand("SELECT COUNT(*) FROM Genders", connection))
            {
                if ((long)cmd.ExecuteScalar() > 0) return;
            }

            var seedCommands = new[]
            {
                "INSERT INTO Genders (Name) VALUES ('Мужской')",
                "INSERT INTO Genders (Name) VALUES ('Женский')",
                "INSERT INTO Citizenships (Name) VALUES ('Российская Федерация')",
                "INSERT INTO Citizenships (Name) VALUES ('Республика Беларусь')",
                "INSERT INTO Citizenships (Name) VALUES ('Республика Казахстан')",
                "INSERT INTO Citizenships (Name) VALUES ('Другое')",
                "INSERT INTO Countries (Name, IsActive) VALUES ('Россия', 1)",
                "INSERT INTO Countries (Name, IsActive) VALUES ('Беларусь', 1)",
                "INSERT INTO Countries (Name, IsActive) VALUES ('Казахстан', 1)",
                "INSERT INTO Countries (Name, IsActive) VALUES ('Украина', 1)",
                "INSERT INTO Countries (Name, IsActive) VALUES ('Другая страна', 1)",
                "INSERT INTO IdentityDocumentTypes (Name) VALUES ('Паспорт гражданина РФ')",
                "INSERT INTO IdentityDocumentTypes (Name) VALUES ('Загранпаспорт')",
                "INSERT INTO IdentityDocumentTypes (Name) VALUES ('Свидетельство о рождении')",
                "INSERT INTO IdentityDocumentTypes (Name) VALUES ('Водительское удостоверение')",
                "INSERT INTO IdentityDocumentTypes (Name) VALUES ('Военный билет')"
            };

            foreach (var commandText in seedCommands)
            {
                using (var cmd = new System.Data.SQLite.SQLiteCommand(commandText, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}