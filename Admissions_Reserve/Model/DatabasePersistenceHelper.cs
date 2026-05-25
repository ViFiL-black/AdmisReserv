using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;

namespace Admissions_Reserve.Model
{
    /// <summary>
    /// Класс-помощник для сохранения и загрузки данных из локальной БД SQLite
    /// Предоставляет общие методы для работы с таблицами, которые используются на разных страницах
    /// </summary>
    public class DatabasePersistenceHelper
    {
        // ========== СОХРАНЕНИЕ И ЗАГРУЗКА ОТНОСИТЕЛЬНЫХ ДОКУМЕНТОВ ==========

        /// <summary>
        /// Загружает относительные документы абитуриента из БД
        /// </summary>
        public static ObservableCollection<RelativeDocument> LoadRelativeDocuments(int applicantId)
        {
            var documents = new ObservableCollection<RelativeDocument>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                var query = @"SELECT * FROM RelativeDocuments 
                             WHERE ApplicantId = @ApplicantId 
                             ORDER BY CreatedAt DESC";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ApplicantId", applicantId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            documents.Add(ReadRelativeDocumentFromReader(reader));
                        }
                    }
                }
            }
            return documents;
        }

        /// <summary>
        /// Сохраняет относительный документ в БД (создание или обновление)
        /// </summary>
        public static int SaveRelativeDocument(int applicantId, string relationDegree, string lastName, 
            string firstName, string patronymic, DateTime? birthDate, string phone, string email,
            string workPlace, string position, string blockReason, bool isBlocked, int? id = null)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    if (id.HasValue && id.Value > 0)
                    {
                        // Обновление существующей записи
                        var query = @"UPDATE RelativeDocuments SET 
                                     RelationDegree = @RelationDegree,
                                     LastName = @LastName,
                                     FirstName = @FirstName,
                                     Patronymic = @Patronymic,
                                     BirthDate = @BirthDate,
                                     Phone = @Phone,
                                     Email = @Email,
                                     WorkPlace = @WorkPlace,
                                     Position = @Position,
                                     BlockReason = @BlockReason,
                                     IsBlocked = @IsBlocked,
                                     UpdatedAt = @UpdatedAt
                                     WHERE Id = @Id AND ApplicantId = @ApplicantId";

                        using (var cmd = new SQLiteCommand(query, connection))
                        {
                            AddRelativeDocumentParameters(cmd, applicantId, relationDegree, lastName, firstName, 
                                patronymic, birthDate, phone, email, workPlace, position, blockReason, isBlocked);
                            cmd.Parameters.AddWithValue("@Id", id.Value);
                            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                        return id.Value;
                    }
                    else
                    {
                        // Создание новой записи
                        var query = @"INSERT INTO RelativeDocuments (
                                     ApplicantId, RelationDegree, LastName, FirstName, Patronymic,
                                     BirthDate, Phone, Email, WorkPlace, Position, BlockReason, IsBlocked,
                                     CreatedAt, UpdatedAt
                                     ) VALUES (
                                     @ApplicantId, @RelationDegree, @LastName, @FirstName, @Patronymic,
                                     @BirthDate, @Phone, @Email, @WorkPlace, @Position, @BlockReason, @IsBlocked,
                                     @CreatedAt, @UpdatedAt
                                     ); SELECT last_insert_rowid();";

                        using (var cmd = new SQLiteCommand(query, connection))
                        {
                            AddRelativeDocumentParameters(cmd, applicantId, relationDegree, lastName, firstName, 
                                patronymic, birthDate, phone, email, workPlace, position, blockReason, isBlocked);
                            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                            var result = cmd.ExecuteScalar();
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при сохранении данных родственника: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Удаляет относительный документ из БД
        /// </summary>
        public static void DeleteRelativeDocument(int id, int applicantId)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    var query = @"DELETE FROM RelativeDocuments WHERE Id = @Id AND ApplicantId = @ApplicantId";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@ApplicantId", applicantId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при удалении данных родственника: " + ex.Message, ex);
            }
        }

        private static void AddRelativeDocumentParameters(SQLiteCommand cmd, int applicantId, string relationDegree,
            string lastName, string firstName, string patronymic, DateTime? birthDate, string phone, string email,
            string workPlace, string position, string blockReason, bool isBlocked)
        {
            cmd.Parameters.AddWithValue("@ApplicantId", applicantId);
            cmd.Parameters.AddWithValue("@RelationDegree", relationDegree ?? "");
            cmd.Parameters.AddWithValue("@LastName", lastName ?? "");
            cmd.Parameters.AddWithValue("@FirstName", firstName ?? "");
            cmd.Parameters.AddWithValue("@Patronymic", patronymic ?? "");
            cmd.Parameters.AddWithValue("@BirthDate", birthDate.HasValue ? (object)birthDate.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Phone", phone ?? "");
            cmd.Parameters.AddWithValue("@Email", email ?? "");
            cmd.Parameters.AddWithValue("@WorkPlace", workPlace ?? "");
            cmd.Parameters.AddWithValue("@Position", position ?? "");
            cmd.Parameters.AddWithValue("@BlockReason", blockReason ?? "");
            cmd.Parameters.AddWithValue("@IsBlocked", isBlocked ? 1 : 0);
        }

        private static RelativeDocument ReadRelativeDocumentFromReader(SQLiteDataReader reader)
        {
            return new RelativeDocument
            {
                Id = Convert.ToInt32(reader["Id"]),
                ApplicantId = Convert.ToInt32(reader["ApplicantId"]),
                RelationDegree = reader["RelationDegree"].ToString(),
                LastName = reader["LastName"].ToString(),
                FirstName = reader["FirstName"].ToString(),
                Patronymic = reader["Patronymic"].ToString(),
                BirthDate = reader["BirthDate"] != DBNull.Value ? Convert.ToDateTime(reader["BirthDate"]) : (DateTime?)null,
                Phone = reader["Phone"].ToString(),
                Email = reader["Email"].ToString(),
                WorkPlace = reader["WorkPlace"].ToString(),
                Position = reader["Position"].ToString(),
                BlockReason = reader["BlockReason"].ToString(),
                IsBlocked = Convert.ToInt32(reader["IsBlocked"]) == 1,
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
            };
        }

        // ========== СОХРАНЕНИЕ И ЗАГРУЗКА ИНДИВИДУАЛЬНЫХ ДОСТИЖЕНИЙ ==========

        /// <summary>
        /// Загружает индивидуальные достижения абитуриента из БД
        /// </summary>
        public static ObservableCollection<IndividualAchievementRecord> LoadIndividualAchievements(int applicantId)
        {
            var achievements = new ObservableCollection<IndividualAchievementRecord>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                var query = @"SELECT * FROM IndividualAchievements 
                             WHERE ApplicantId = @ApplicantId 
                             ORDER BY CreatedAt DESC";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ApplicantId", applicantId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            achievements.Add(ReadIndividualAchievementFromReader(reader));
                        }
                    }
                }
            }
            return achievements;
        }

        /// <summary>
        /// Сохраняет индивидуальное достижение в БД
        /// </summary>
        public static int SaveIndividualAchievement(int applicantId, string category, string achievementName, 
            string year, int points, string documentName, string documentPath, int? id = null)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    if (id.HasValue && id.Value > 0)
                    {
                        // Обновление
                        var query = @"UPDATE IndividualAchievements SET 
                                     Category = @Category,
                                     AchievementName = @AchievementName,
                                     Year = @Year,
                                     Points = @Points,
                                     DocumentName = @DocumentName,
                                     DocumentPath = @DocumentPath,
                                     UpdatedAt = @UpdatedAt
                                     WHERE Id = @Id AND ApplicantId = @ApplicantId";

                        using (var cmd = new SQLiteCommand(query, connection))
                        {
                            AddIndividualAchievementParameters(cmd, applicantId, category, achievementName, 
                                year, points, documentName, documentPath);
                            cmd.Parameters.AddWithValue("@Id", id.Value);
                            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                        return id.Value;
                    }
                    else
                    {
                        // Создание
                        var query = @"INSERT INTO IndividualAchievements (
                                     ApplicantId, Category, AchievementName, Year, Points,
                                     DocumentName, DocumentPath, CreatedAt, UpdatedAt
                                     ) VALUES (
                                     @ApplicantId, @Category, @AchievementName, @Year, @Points,
                                     @DocumentName, @DocumentPath, @CreatedAt, @UpdatedAt
                                     ); SELECT last_insert_rowid();";

                        using (var cmd = new SQLiteCommand(query, connection))
                        {
                            AddIndividualAchievementParameters(cmd, applicantId, category, achievementName, 
                                year, points, documentName, documentPath);
                            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                            var result = cmd.ExecuteScalar();
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при сохранении индивидуального достижения: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Удаляет индивидуальное достижение из БД
        /// </summary>
        public static void DeleteIndividualAchievement(int id, int applicantId)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    var query = @"DELETE FROM IndividualAchievements WHERE Id = @Id AND ApplicantId = @ApplicantId";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@ApplicantId", applicantId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при удалении индивидуального достижения: " + ex.Message, ex);
            }
        }

        private static void AddIndividualAchievementParameters(SQLiteCommand cmd, int applicantId, 
            string category, string achievementName, string year, int points, string documentName, string documentPath)
        {
            cmd.Parameters.AddWithValue("@ApplicantId", applicantId);
            cmd.Parameters.AddWithValue("@Category", category ?? "");
            cmd.Parameters.AddWithValue("@AchievementName", achievementName ?? "");
            cmd.Parameters.AddWithValue("@Year", year ?? "");
            cmd.Parameters.AddWithValue("@Points", points);
            cmd.Parameters.AddWithValue("@DocumentName", documentName ?? "");
            cmd.Parameters.AddWithValue("@DocumentPath", documentPath ?? "");
        }

        private static IndividualAchievementRecord ReadIndividualAchievementFromReader(SQLiteDataReader reader)
        {
            return new IndividualAchievementRecord
            {
                Id = Convert.ToInt32(reader["Id"]),
                ApplicantId = Convert.ToInt32(reader["ApplicantId"]),
                Category = reader["Category"].ToString(),
                AchievementName = reader["AchievementName"].ToString(),
                Year = reader["Year"].ToString(),
                Points = Convert.ToInt32(reader["Points"]),
                DocumentName = reader["DocumentName"].ToString(),
                DocumentPath = reader["DocumentPath"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
            };
        }

        // ========== СОХРАНЕНИЕ И ЗАГРУЗКА ПРИЛОЖЕННЫХ ДОКУМЕНТОВ ==========

        /// <summary>
        /// Загружает приложенные документы абитуриента из БД
        /// </summary>
        public static ObservableCollection<AttachedDocumentRecord> LoadAttachedDocuments(int applicantId)
        {
            var documents = new ObservableCollection<AttachedDocumentRecord>();

            using (var connection = DatabaseHelper.GetConnection())
            {
                var query = @"SELECT * FROM AttachedDocuments 
                             WHERE ApplicantId = @ApplicantId 
                             ORDER BY CreatedAt DESC";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@ApplicantId", applicantId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            documents.Add(ReadAttachedDocumentFromReader(reader));
                        }
                    }
                }
            }
            return documents;
        }

        /// <summary>
        /// Сохраняет приложенный документ в БД
        /// </summary>
        public static int SaveAttachedDocument(int applicantId, string documentType, string seriesNumber, 
            string category, string additionalData, DateTime? issueDate, string documentInfo, 
            string attachmentPath, string attachmentName, int? id = null)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    if (id.HasValue && id.Value > 0)
                    {
                        // Обновление
                        var query = @"UPDATE AttachedDocuments SET 
                                     DocumentType = @DocumentType,
                                     SeriesNumber = @SeriesNumber,
                                     Category = @Category,
                                     AdditionalData = @AdditionalData,
                                     IssueDate = @IssueDate,
                                     DocumentInfo = @DocumentInfo,
                                     AttachmentPath = @AttachmentPath,
                                     AttachmentName = @AttachmentName,
                                     UpdatedAt = @UpdatedAt
                                     WHERE Id = @Id AND ApplicantId = @ApplicantId";

                        using (var cmd = new SQLiteCommand(query, connection))
                        {
                            AddAttachedDocumentParameters(cmd, applicantId, documentType, seriesNumber, 
                                category, additionalData, issueDate, documentInfo, attachmentPath, attachmentName);
                            cmd.Parameters.AddWithValue("@Id", id.Value);
                            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }
                        return id.Value;
                    }
                    else
                    {
                        // Создание
                        var query = @"INSERT INTO AttachedDocuments (
                                     ApplicantId, DocumentType, SeriesNumber, Category, AdditionalData,
                                     IssueDate, DocumentInfo, AttachmentPath, AttachmentName, CreatedAt, UpdatedAt
                                     ) VALUES (
                                     @ApplicantId, @DocumentType, @SeriesNumber, @Category, @AdditionalData,
                                     @IssueDate, @DocumentInfo, @AttachmentPath, @AttachmentName, @CreatedAt, @UpdatedAt
                                     ); SELECT last_insert_rowid();";

                        using (var cmd = new SQLiteCommand(query, connection))
                        {
                            AddAttachedDocumentParameters(cmd, applicantId, documentType, seriesNumber, 
                                category, additionalData, issueDate, documentInfo, attachmentPath, attachmentName);
                            cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                            cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
                            var result = cmd.ExecuteScalar();
                            return Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при сохранении приложенного документа: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Удаляет приложенный документ из БД
        /// </summary>
        public static void DeleteAttachedDocument(int id, int applicantId)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    var query = @"DELETE FROM AttachedDocuments WHERE Id = @Id AND ApplicantId = @ApplicantId";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        cmd.Parameters.AddWithValue("@ApplicantId", applicantId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при удалении приложенного документа: " + ex.Message, ex);
            }
        }

        private static void AddAttachedDocumentParameters(SQLiteCommand cmd, int applicantId, string documentType, 
            string seriesNumber, string category, string additionalData, DateTime? issueDate, string documentInfo, 
            string attachmentPath, string attachmentName)
        {
            cmd.Parameters.AddWithValue("@ApplicantId", applicantId);
            cmd.Parameters.AddWithValue("@DocumentType", documentType ?? "");
            cmd.Parameters.AddWithValue("@SeriesNumber", seriesNumber ?? "");
            cmd.Parameters.AddWithValue("@Category", category ?? "");
            cmd.Parameters.AddWithValue("@AdditionalData", additionalData ?? "");
            cmd.Parameters.AddWithValue("@IssueDate", issueDate.HasValue ? (object)issueDate.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@DocumentInfo", documentInfo ?? "");
            cmd.Parameters.AddWithValue("@AttachmentPath", attachmentPath ?? "");
            cmd.Parameters.AddWithValue("@AttachmentName", attachmentName ?? "");
        }

        private static AttachedDocumentRecord ReadAttachedDocumentFromReader(SQLiteDataReader reader)
        {
            return new AttachedDocumentRecord
            {
                Id = Convert.ToInt32(reader["Id"]),
                ApplicantId = Convert.ToInt32(reader["ApplicantId"]),
                DocumentType = reader["DocumentType"].ToString(),
                SeriesNumber = reader["SeriesNumber"].ToString(),
                Category = reader["Category"].ToString(),
                AdditionalData = reader["AdditionalData"].ToString(),
                IssueDate = reader["IssueDate"] != DBNull.Value ? Convert.ToDateTime(reader["IssueDate"]) : (DateTime?)null,
                DocumentInfo = reader["DocumentInfo"].ToString(),
                AttachmentPath = reader["AttachmentPath"].ToString(),
                AttachmentName = reader["AttachmentName"].ToString(),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
            };
        }
    }

    // ========== МОДЕЛИ ДАННЫХ ==========

    public class RelativeDocument
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public string RelationDegree { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string WorkPlace { get; set; }
        public string Position { get; set; }
        public string BlockReason { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class IndividualAchievementRecord
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public string Category { get; set; }
        public string AchievementName { get; set; }
        public string Year { get; set; }
        public int Points { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AttachedDocumentRecord
    {
        public int Id { get; set; }
        public int ApplicantId { get; set; }
        public string DocumentType { get; set; }
        public string SeriesNumber { get; set; }
        public string Category { get; set; }
        public string AdditionalData { get; set; }
        public DateTime? IssueDate { get; set; }
        public string DocumentInfo { get; set; }
        public string AttachmentPath { get; set; }
        public string AttachmentName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
