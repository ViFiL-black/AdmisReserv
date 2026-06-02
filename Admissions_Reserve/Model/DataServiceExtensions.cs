// Этот файл содержит расширения для класса DataService
// Добавить содержимое в конец файла DataService.cs

namespace Admissions_Reserve.Model
{
    public static partial class DataServiceExtensions
    {
        /// <summary>
        /// Обновляет общий документ (Documents) с сохранением дополнительных данных
        /// </summary>
        public static void UpdateGeneralDocument(Documents doc)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                var query = @"UPDATE Documents SET 
                    DocumentTypeId = @DocumentTypeId,
                    Series = @Series,
                    Number = @Number,
                    AdditionalData = @AdditionalData,
                    DocumentInfo = @DocumentInfo,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id";

                using (var cmd = new System.Data.SQLite.SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Id", doc.Id);
                    cmd.Parameters.AddWithValue("@DocumentTypeId", (object)doc.DocumentTypeId ?? System.DBNull.Value);
                    cmd.Parameters.AddWithValue("@Series", (object)doc.Series ?? System.DBNull.Value);
                    cmd.Parameters.AddWithValue("@Number", (object)doc.Number ?? System.DBNull.Value);
                    cmd.Parameters.AddWithValue("@AdditionalData", (object)doc.AdditionalData ?? System.DBNull.Value);
                    cmd.Parameters.AddWithValue("@DocumentInfo", (object)doc.DocumentInfo ?? System.DBNull.Value);
                    cmd.Parameters.AddWithValue("@UpdatedAt", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
    }
}
