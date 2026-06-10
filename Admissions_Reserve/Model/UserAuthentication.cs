using System;
using System.Data.SQLite;

namespace Admissions_Reserve.Model
{
    public static class UserAuthentication
    {
        private static int? _currentUserId;
        private static string _currentUserFullName;
        private static string _currentUserRole;

        public static int? CurrentUserId => _currentUserId;
        public static string CurrentUserFullName => _currentUserFullName;
        public static string CurrentUserRole => _currentUserRole;

        /// <summary>
        /// Аутентификация пользователя по логину и паролю (прямое сравнение строк).
        /// </summary>
        public static bool Authenticate(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                return false;

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"
                        SELECT u.Id, u.Password, u.FullName, u.RoleId, r.Name AS RoleName
                        FROM Users u
                        LEFT JOIN Roles r ON u.RoleId = r.Id
                        WHERE u.Login = @Login";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Login", login);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedPassword = reader["Password"]?.ToString();
                                string fullName = reader["FullName"]?.ToString();
                                int userId = Convert.ToInt32(reader["Id"]);
                                string roleName = reader["RoleName"]?.ToString();

                                if (VerifyPassword(password, storedPassword))
                                {
                                    _currentUserId = userId;
                                    _currentUserFullName = fullName;
                                    _currentUserRole = roleName ?? "User";
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError("Ошибка при аутентификации пользователя", ex);
            }

            return false;
        }

        public static void Logout()
        {
            _currentUserId = null;
            _currentUserFullName = null;
            _currentUserRole = null;
        }

        /// <summary>
        /// Прямое сравнение пароля (без хэширования).
        /// </summary>
        private static bool VerifyPassword(string inputPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedPassword))
                return false;
            return inputPassword == storedPassword;
        }

        /// <summary>
        /// Хэширование пароля (оставлено для возможного использования в будущем, но в аутентификации не применяется).
        /// </summary>
        public static string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Создание нового пользователя с указанием роли (пароль сохраняется в открытом виде).
        /// </summary>
        public static bool CreateUser(string login, string password, string fullName, string roleName = "User")
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullName))
                return false;

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    int? roleId = null;
                    if (!string.IsNullOrEmpty(roleName))
                    {
                        using (var roleCmd = new SQLiteCommand("SELECT Id FROM Roles WHERE Name = @RoleName", connection))
                        {
                            roleCmd.Parameters.AddWithValue("@RoleName", roleName);
                            var result = roleCmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                                roleId = Convert.ToInt32(result);
                        }
                    }

                    string query = @"
                        INSERT INTO Users (Login, Password, FullName, CreatedAt, RoleId)
                        VALUES (@Login, @Password, @FullName, @CreatedAt, @RoleId)";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Login", login);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@FullName", fullName);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.Parameters.AddWithValue("@RoleId", roleId ?? (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError("Ошибка при создании пользователя", ex);
                return false;
            }
        }

        /// <summary>
        /// Возвращает объект текущего пользователя (без пароля).
        /// </summary>
        public static User GetCurrentUser()
        {
            if (!_currentUserId.HasValue)
                return null;

            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    string query = @"
                        SELECT u.Id, u.Login, u.FullName, u.CreatedAt, u.RoleId, r.Name AS RoleName
                        FROM Users u
                        LEFT JOIN Roles r ON u.RoleId = r.Id
                        WHERE u.Id = @Id";

                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", _currentUserId.Value);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    Id = Convert.ToInt32(reader["Id"]),
                                    Login = reader["Login"]?.ToString(),
                                    FullName = reader["FullName"]?.ToString(),
                                    CreatedAt = DateTime.Parse(reader["CreatedAt"]?.ToString() ?? DateTime.Now.ToString()),
                                    RoleId = reader["RoleId"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["RoleId"]),
                                    RoleName = reader["RoleName"]?.ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError("Ошибка при получении текущего пользователя", ex);
            }

            return null;
        }

        /// <summary>
        /// Получение имени пользователя по ID.
        /// </summary>
        public static string GetUserNameById(int userId)
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    var query = "SELECT FullName FROM Users WHERE Id = @Id";
                    using (var cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", userId);
                        var result = cmd.ExecuteScalar();
                        return result?.ToString() ?? "Неизвестный пользователь";
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogError("Ошибка при получении имени пользователя", ex);
                return "Неизвестный пользователь";
            }
        }

        /// <summary>
        /// Проверка, является ли текущий пользователь администратором.
        /// </summary>
        public static bool IsCurrentUserAdmin()
        {
            return !string.IsNullOrEmpty(_currentUserRole) && _currentUserRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Модель пользователя (расширенная).
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }   // поле может использоваться при создании
        public string FullName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? RoleId { get; set; }
        public string RoleName { get; set; }
    }
}