// SessionManager.cs
using System;
using System.Data.SQLite;

namespace Admissions_Reserve.Model
{
    public static class SessionManager
    {
        private static Applicants _currentApplicant;
        private static int? _currentApplicantId;

        public static Applicants CurrentApplicant
        {
            get
            {
                if (_currentApplicant == null && _currentApplicantId.HasValue)
                {
                    _currentApplicant = LoadApplicant(_currentApplicantId.Value);
                }
                return _currentApplicant;
            }
            set
            {
                _currentApplicant = value;
                _currentApplicantId = value?.Id;
            }
        }

        public static int? CurrentApplicantId => _currentApplicantId;

        public static void Clear()
        {
            _currentApplicant = null;
            _currentApplicantId = null;
        }

        public static Applicants LoadApplicant(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                var query = "SELECT * FROM Applicants WHERE Id = @Id";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadApplicantFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        private static Applicants ReadApplicantFromReader(SQLiteDataReader reader)
        {
            return new Applicants
            {
                Id = Convert.ToInt32(reader["Id"]),
                LastName = reader["LastName"]?.ToString(),
                FirstName = reader["FirstName"]?.ToString(),
                Patronymic = reader["Patronymic"]?.ToString(),
                BirthDate = (DateTime)(reader["BirthDate"] != DBNull.Value ? Convert.ToDateTime(reader["BirthDate"]) : (DateTime?)null),
                BirthPlace = reader["BirthPlace"]?.ToString(),
                GenderId = reader["GenderId"] != DBNull.Value ? Convert.ToInt32(reader["GenderId"]) : (int?)null,
                CitizenshipId = reader["CitizenshipId"] != DBNull.Value ? Convert.ToInt32(reader["CitizenshipId"]) : (int?)null,
                Snils = reader["Snils"]?.ToString(),
                RegistrationCountryId = reader["RegistrationCountryId"] != DBNull.Value ? Convert.ToInt32(reader["RegistrationCountryId"]) : (int?)null,
                RegistrationPostalCode = reader["RegistrationPostalCode"]?.ToString(),
                RegistrationRegion = reader["RegistrationRegion"]?.ToString(),
                RegistrationDistrict = reader["RegistrationDistrict"]?.ToString(),
                RegistrationCity = reader["RegistrationCity"]?.ToString(),
                RegistrationStreet = reader["RegistrationStreet"]?.ToString(),
                RegistrationHouse = reader["RegistrationHouse"]?.ToString(),
                RegistrationBuilding = reader["RegistrationBuilding"]?.ToString(),
                RegistrationApartment = reader["RegistrationApartment"]?.ToString(),
                ActualAddressSame = reader["ActualAddressSame"] != DBNull.Value ? Convert.ToBoolean(reader["ActualAddressSame"]) : (bool?)null,
                ActualCountryId = reader["ActualCountryId"] != DBNull.Value ? Convert.ToInt32(reader["ActualCountryId"]) : (int?)null,
                ActualPostalCode = reader["ActualPostalCode"]?.ToString(),
                ActualRegion = reader["ActualRegion"]?.ToString(),
                ActualDistrict = reader["ActualDistrict"]?.ToString(),
                ActualCity = reader["ActualCity"]?.ToString(),
                ActualStreet = reader["ActualStreet"]?.ToString(),
                ActualHouse = reader["ActualHouse"]?.ToString(),
                ActualBuilding = reader["ActualBuilding"]?.ToString(),
                ActualApartment = reader["ActualApartment"]?.ToString(),
                Phone = reader["Phone"]?.ToString(),
                MobilePhone = reader["MobilePhone"]?.ToString(),
                Fax = reader["Fax"]?.ToString(),
                WorkPhone = reader["WorkPhone"]?.ToString(),
                Email = reader["Email"]?.ToString(),
                AdditionalEmail = reader["AdditionalEmail"]?.ToString(),
                Website = reader["Website"]?.ToString(),
                Telegram = reader["Telegram"]?.ToString(),
                WhatsApp = reader["WhatsApp"]?.ToString(),
                Viber = reader["Viber"]?.ToString(),
                PreferredContactMethod = reader["PreferredContactMethod"]?.ToString(),
                ContactComment = reader["ContactComment"]?.ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
            };
        }
    }
}