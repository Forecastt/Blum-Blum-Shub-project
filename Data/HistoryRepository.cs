using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blum_Blum_Shub_project.Data
{
    public sealed class HistoryRepository
    {
        private readonly SqliteDb _db;

        public HistoryRepository(SqliteDb db)
        {
            _db = db;
            _db.EnsureCreated();
        }

        public void Add(string userLogin, string action, string? info = null)
        {
            try
            {
                using var conn = _db.CreateConnection();
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
INSERT INTO History(UserLogin, Action, Info, CreatedAt)
VALUES(@u, @a, @i, @t);";
                cmd.Parameters.AddWithValue("@u", userLogin);
                cmd.Parameters.AddWithValue("@a", action);
                cmd.Parameters.AddWithValue("@i", (object?)info ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@t", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Error("History.Add failed", ex);
            }
        }

        public List<(string Action, string Info, string CreatedAt)> GetForUser(string userLogin)
        {
            var list = new List<(string, string, string)>();

            try
            {
                using var conn = _db.CreateConnection();
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
SELECT Action, COALESCE(Info,''), CreatedAt
FROM History
WHERE UserLogin = @u
ORDER BY Id DESC;";
                cmd.Parameters.AddWithValue("@u", userLogin);

                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    list.Add((r.GetString(0), r.GetString(1), r.GetString(2)));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("History.GetForUser failed", ex);
            }

            return list;
        }

        public void ClearForUser(string userLogin)
        {
            try
            {
                using var conn = _db.CreateConnection();
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM History WHERE UserLogin = @u;";
                cmd.Parameters.AddWithValue("@u", userLogin);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.Error("History.ClearForUser failed", ex);
            }
        }
    }
}
