using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blum_Blum_Shub_project.Data
{
    public sealed class LoginLockRepository
    {
        private readonly SqliteDb _db;

        public LoginLockRepository(SqliteDb db)
        {
            _db = db;
            _db.EnsureCreated();
        }

        public (int FailCount, DateTime? LockedUntil) Get(string login)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT FailCount, LockedUntil FROM LoginLock WHERE Login=@l;";
            cmd.Parameters.AddWithValue("@l", login);

            using var r = cmd.ExecuteReader();
            if (!r.Read()) return (0, null);

            int fail = r.GetInt32(0);
            DateTime? until = null;

            if (!r.IsDBNull(1))
            {
                var s = r.GetString(1);
                if (DateTime.TryParse(s, out var dt)) until = dt;
            }

            return (fail, until);
        }

        public void Reset(string login)
        {
            using var conn = _db.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO LoginLock(Login, FailCount, LockedUntil)
VALUES(@l, 0, NULL)
ON CONFLICT(Login) DO UPDATE SET FailCount=0, LockedUntil=NULL;";
            cmd.Parameters.AddWithValue("@l", login);
            cmd.ExecuteNonQuery();
        }

        public void RegisterFail(string login)
        {
            var (fail, until) = Get(login);

            if (until != null && DateTime.Now >= until.Value)
            {
                fail = 0;
                until = null;
            }

            fail++;

            DateTime? newUntil = null;
            if (fail >= 3) newUntil = DateTime.Now.AddSeconds(30);

            using var conn = _db.CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO LoginLock(Login, FailCount, LockedUntil)
VALUES(@l, @f, @u)
ON CONFLICT(Login) DO UPDATE SET FailCount=@f, LockedUntil=@u;";
            cmd.Parameters.AddWithValue("@l", login);
            cmd.Parameters.AddWithValue("@f", fail);
            cmd.Parameters.AddWithValue("@u", (object?)newUntil?.ToString("yyyy-MM-dd HH:mm:ss") ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }
    }
}
