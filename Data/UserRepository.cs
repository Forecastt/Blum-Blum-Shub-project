using Blum_Blum_Shub_project.Core;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blum_Blum_Shub_project.Data
{
    public sealed class UserRepository
    {
        private readonly SqliteDb _db;

        public UserRepository(SqliteDb db)
        {
            _db = db;
            _db.EnsureCreated();
        }

        public bool UserExists(string login)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM Users WHERE Login = @login LIMIT 1;";
            cmd.Parameters.AddWithValue("@login", login);

            var res = cmd.ExecuteScalar();
            return res != null;
        }

        public bool Register(string login, string password, out string error)
        {
            error = "";

            if (UserExists(login))
            {
                error = "Такой логин уже есть.";
                return false;
            }

            string salt = PasswordHasher.GenerateSalt();
            string hash = PasswordHasher.HashPassword(password, salt);

            using var conn = _db.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO Users(Login, PasswordHash, Salt, CreatedAt)
VALUES(@login, @hash, @salt, @created);";

            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@salt", salt);
            cmd.Parameters.AddWithValue("@created", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            try
            {
                return cmd.ExecuteNonQuery() == 1;
            }
            catch (SqliteException ex)
            {
                error = "Ошибка БД: " + ex.Message;
                return false;
            }
        }

        public bool TryLogin(string login, string password)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT PasswordHash, Salt FROM Users WHERE Login = @login LIMIT 1;";
            cmd.Parameters.AddWithValue("@login", login);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return false;

            string hash = reader.GetString(0);
            string salt = reader.GetString(1);

            return PasswordHasher.Verify(password, salt, hash);
        }
    }
}
