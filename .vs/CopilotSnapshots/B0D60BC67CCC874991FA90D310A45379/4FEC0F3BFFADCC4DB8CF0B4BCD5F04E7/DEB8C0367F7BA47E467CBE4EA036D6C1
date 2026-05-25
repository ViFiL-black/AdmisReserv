// DataService.cs
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace Admissions_Reserve.Model
{
    public class DataService
    {
        // ========== МЕТОДЫ ДЛЯ АБИТУРИЕНТОВ ==========

        public static int CreateApplicant(Applicants applicant)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                var query = @"INSERT INTO Applicants (
                    LastName, FirstName, Patronymic, BirthDate, BirthPlace,
                    GenderId, CitizenshipId, Snils,
                    RegistrationCountryId, RegistrationPostalCode, RegistrationRegion,
                    RegistrationDistrict, RegistrationCity, RegistrationStreet,
                    RegistrationHouse, RegistrationBuilding, RegistrationApartment,
                    ActualAddressSame, ActualCountryId, ActualPostalCode,
                    ActualRegion, ActualDistrict, ActualCity, ActualStreet,
                    ActualHouse, ActualBuilding, ActualApartment,
                    Phone, MobilePhone, Fax, WorkPhone, Email, AdditionalEmail,
                    Website, Telegram, WhatsApp, Viber,
                    PreferredContactMethod, ContactComment,
                    CreatedAt, UpdatedAt
                ) VALUES (
                    @LastName, @FirstName, @Patronymic, @BirthDate, @BirthPlace,
                    @GenderId, @CitizenshipId, @Snils,
                    @RegistrationCountryId, @RegistrationPostalCode, @RegistrationRegion,
                    @RegistrationDistrict, @RegistrationCity, @RegistrationStreet,
                    @RegistrationHouse, @RegistrationBuilding, @RegistrationApartment,
                    @ActualAddressSame, @ActualCountryId, @ActualPostalCode,
                    @ActualRegion, @ActualDistrict, @ActualCity, @ActualStreet,
                    @ActualHouse, @ActualBuilding, @ActualApartment,
                    @Phone, @MobilePhone, @Fax, @WorkPhone, @Email, @AdditionalEmail,
                    @Website, @Telegram, @WhatsApp, @Viber,
                    @PreferredContactMethod, @ContactComment,
                    @CreatedAt, @UpdatedAt
                ); SELECT last_insert_rowid();";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    AddApplicantParameters(cmd, applicant);
                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        public static void UpdateApplicant(Applicants applicant)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                var query = @"UPDATE Applicants SET 
                    LastName = @LastName, FirstName = @FirstName, Patronymic = @Patronymic,
                    BirthDate = @BirthDate, BirthPlace = @BirthPlace,
                    GenderId = @GenderId, CitizenshipId = @CitizenshipId, Snils = @Snils,
                    RegistrationCountryId = @RegistrationCountryId,
                    RegistrationPostalCode = @RegistrationPostalCode,
                    RegistrationRegion = @RegistrationRegion,
                    RegistrationDistrict = @RegistrationDistrict,
                    RegistrationCity = @RegistrationCity,
                    RegistrationStreet = @RegistrationStreet,
                    RegistrationHouse = @RegistrationHouse,
                    RegistrationBuilding = @RegistrationBuilding,
                    RegistrationApartment = @RegistrationApartment,
                    ActualAddressSame = @ActualAddressSame,
                    ActualCountryId = @ActualCountryId,
                    ActualPostalCode = @ActualPostalCode,
                    ActualRegion = @ActualRegion,
                    ActualDistrict = @ActualDistrict,
                    ActualCity = @ActualCity,
                    ActualStreet = @ActualStreet,
                    ActualHouse = @ActualHouse,
                    ActualBuilding = @ActualBuilding,
                    ActualApartment = @ActualApartment,
                    Phone = @Phone, MobilePhone = @MobilePhone, Fax = @Fax,
                    WorkPhone = @WorkPhone, Email = @Email, AdditionalEmail = @AdditionalEmail,
                    Website = @Website, Telegram = @Telegram, WhatsApp = @WhatsApp,
                    Viber = @Viber, PreferredContactMethod = @PreferredContactMethod,
                    ContactComment = @ContactComment, UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", applicant.Id);
                    AddApplicantParameters(cmd, applicant);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static Applicants GetApplicant(int id)
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

        public static void DeleteApplicant(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                // Сначала удаляем связанные документы
                var deleteDocs = "DELETE FROM IdentityDocuments WHERE ApplicantId = @Id";
                using (var cmd = new SQLiteCommand(deleteDocs, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }

                // Затем удаляем абитуриента
                var deleteApplicant = "DELETE FROM Applicants WHERE Id = @Id";
                using (var cmd = new SQLiteCommand(deleteApplicant, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void AddApplicantParameters(SQLiteCommand cmd, Applicants applicant)
        {
            cmd.Parameters.AddWithValue("@LastName", (object)applicant.LastName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FirstName", (object)applicant.FirstName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Patronymic", (object)applicant.Patronymic ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BirthDate", (object)applicant.BirthDate.ToString("yyyy-MM-dd HH:mm:ss") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BirthPlace", (object)applicant.BirthPlace ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GenderId", (object)applicant.GenderId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CitizenshipId", (object)applicant.CitizenshipId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Snils", (object)applicant.Snils ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RegistrationCountryId", (object)applicant.RegistrationCountryId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RegistrationPostalCode", (object)applicant.RegistrationPostalCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RegistrationRegion", (object)applicant.RegistrationRegion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RegistrationDistrict", (object)applicant.RegistrationDistrict ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RegistrationCity", (object)applicant.RegistrationCity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RegistrationStreet", (object)applicant.RegistrationStreet ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RegistrationHouse", (object)applicant.RegistrationHouse ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RegistrationBuilding", (object)applicant.RegistrationBuilding ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RegistrationApartment", (object)applicant.RegistrationApartment ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActualAddressSame", (object)applicant.ActualAddressSame ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActualCountryId", (object)applicant.ActualCountryId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActualPostalCode", (object)applicant.ActualPostalCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActualRegion", (object)applicant.ActualRegion ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActualDistrict", (object)applicant.ActualDistrict ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActualCity", (object)applicant.ActualCity ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActualStreet", (object)applicant.ActualStreet ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActualHouse", (object)applicant.ActualHouse ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActualBuilding", (object)applicant.ActualBuilding ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActualApartment", (object)applicant.ActualApartment ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", (object)applicant.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MobilePhone", (object)applicant.MobilePhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Fax", (object)applicant.Fax ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WorkPhone", (object)applicant.WorkPhone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", (object)applicant.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AdditionalEmail", (object)applicant.AdditionalEmail ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Website", (object)applicant.Website ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Telegram", (object)applicant.Telegram ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WhatsApp", (object)applicant.WhatsApp ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Viber", (object)applicant.Viber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PreferredContactMethod", (object)applicant.PreferredContactMethod ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ContactComment", (object)applicant.ContactComment ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedAt", (object)applicant.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UpdatedAt", (object)applicant.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
        }

        private static Applicants ReadApplicantFromReader(SQLiteDataReader reader)
        {
            return new Applicants
            {
                Id = Convert.ToInt32(reader["Id"]),
                LastName = reader["LastName"]?.ToString(),
                FirstName = reader["FirstName"]?.ToString(),
                Patronymic = reader["Patronymic"]?.ToString(),
                BirthDate = reader["BirthDate"] != DBNull.Value ? Convert.ToDateTime(reader["BirthDate"]) : DateTime.MinValue,
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
                CreatedAt = reader["CreatedAt"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["CreatedAt"]) : null,
                UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["UpdatedAt"]) : null
            };
        }

        // ========== МЕТОДЫ ДЛЯ СПРАВОЧНИКОВ ==========

        public static List<T> GetAll<T>() where T : new()
        {
            var result = new List<T>();
            var tableName = typeof(T).Name;

            using (var connection = DatabaseHelper.GetConnection())
            {
                var query = $"SELECT * FROM {tableName}";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(ReadEntity<T>(reader));
                        }
                    }
                }
            }
            return result;
        }

        public static List<T> GetByCondition<T>(string whereClause, params SQLiteParameter[] parameters) where T : new()
        {
            var result = new List<T>();
            var tableName = typeof(T).Name;

            using (var connection = DatabaseHelper.GetConnection())
            {
                var query = $"SELECT * FROM {tableName} WHERE {whereClause}";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddRange(parameters);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(ReadEntity<T>(reader));
                        }
                    }
                }
            }
            return result;
        }

        private static T ReadEntity<T>(SQLiteDataReader reader) where T : new()
        {
            var entity = new T();
            var type = typeof(T);

            foreach (var prop in type.GetProperties())
            {
                try
                {
                    if (reader[prop.Name] != DBNull.Value)
                    {
                        prop.SetValue(entity, Convert.ChangeType(reader[prop.Name], prop.PropertyType));
                    }
                }
                catch
                {
                    // Пропускаем поля, которые не удалось прочитать
                }
            }

            return entity;
        }

        // ========== МЕТОДЫ ДЛЯ ДОКУМЕНТОВ ==========

        public static int CreateIdentityDocument(IdentityDocuments doc)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                var query = @"INSERT INTO IdentityDocuments (
                    ApplicantId, DocumentTypeId, Series, Number,
                    IssuedBy, IssueDate, DepartmentCode, IsPrimary, AddedDate
                ) VALUES (
                    @ApplicantId, @DocumentTypeId, @Series, @Number,
                    @IssuedBy, @IssueDate, @DepartmentCode, @IsPrimary, @AddedDate
                ); SELECT last_insert_rowid();";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    AddDocumentParameters(cmd, doc);
                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        public static void UpdateIdentityDocument(IdentityDocuments doc)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                var query = @"UPDATE IdentityDocuments SET 
                    DocumentTypeId = @DocumentTypeId,
                    Series = @Series,
                    Number = @Number,
                    IssuedBy = @IssuedBy,
                    IssueDate = @IssueDate,
                    DepartmentCode = @DepartmentCode,
                    IsPrimary = @IsPrimary
                WHERE Id = @Id";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", doc.Id);
                    AddDocumentParameters(cmd, doc);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public static List<IdentityDocuments> GetApplicantDocuments(int applicantId)
        {
            var docs = new List<IdentityDocuments>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                var query = "SELECT * FROM IdentityDocuments WHERE ApplicantId = @ApplicantId";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ApplicantId", applicantId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            docs.Add(ReadDocumentFromReader(reader));
                        }
                    }
                }
            }
            return docs;
        }

        public static IdentityDocuments GetDocument(int id)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                var query = "SELECT * FROM IdentityDocuments WHERE Id = @Id";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadDocumentFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        private static void AddDocumentParameters(SQLiteCommand cmd, IdentityDocuments doc)
        {
            cmd.Parameters.AddWithValue("@ApplicantId", doc.ApplicantId);
            cmd.Parameters.AddWithValue("@DocumentTypeId", (object)doc.DocumentTypeId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Series", (object)doc.Series ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Number", (object)doc.Number ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IssuedBy", (object)doc.IssuedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IssueDate", (object)doc.IssueDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DepartmentCode", (object)doc.DepartmentCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsPrimary", (object)doc.IsPrimary ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AddedDate", (object)doc.AddedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
        }

        private static IdentityDocuments ReadDocumentFromReader(SQLiteDataReader reader)
        {
            return new IdentityDocuments
            {
                Id = Convert.ToInt32(reader["Id"]),
                ApplicantId = Convert.ToInt32(reader["ApplicantId"]),
                DocumentTypeId = reader["DocumentTypeId"] != DBNull.Value ? Convert.ToInt32(reader["DocumentTypeId"]) : (int?)null,
                Series = reader["Series"]?.ToString(),
                Number = reader["Number"]?.ToString(),
                IssuedBy = reader["IssuedBy"]?.ToString(),
                IssueDate = reader["IssueDate"] != DBNull.Value ? Convert.ToDateTime(reader["IssueDate"]) : (DateTime?)null,
                DepartmentCode = reader["DepartmentCode"]?.ToString(),
                IsPrimary = reader["IsPrimary"] != DBNull.Value ? Convert.ToBoolean(reader["IsPrimary"]) : (bool?)null,
                AddedDate = Convert.ToDateTime(reader["AddedDate"])
            };
        }

        // ========== МЕТОДЫ ДЛЯ ЛОГИРОВАНИЯ ==========

        public static void LogChange(string tableName, int recordId, string action)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    var query = @"INSERT INTO ChangeHistory (TableName, RecordId, Action, ChangedAt) 
                                  VALUES (@TableName, @RecordId, @Action, @ChangedAt)";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@TableName", tableName);
                        cmd.Parameters.AddWithValue("@RecordId", recordId);
                        cmd.Parameters.AddWithValue("@Action", action);
                        cmd.Parameters.AddWithValue("@ChangedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
        }
    }
}