using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace Blum_Blum_Shub_project.Data
{
    public sealed class SqliteDb
    {
        public string DbPath { get; }
        private readonly string _cs;

        public SqliteDb()
        {
#if DEBUG
            DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BBS.db");
#else
            Directory.CreateDirectory(Logger.BaseDir);
            DbPath = Path.Combine(Logger.BaseDir, "BBS.db");
#endif
            _cs = new SqliteConnectionStringBuilder { DataSource = DbPath }.ToString();
        }

        public SqliteConnection CreateConnection() => new SqliteConnection(_cs);

        public void EnsureCreated()
        {
            Logger.EnsureLogsFolder();

            using var conn = CreateConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Users(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Login TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Salt TEXT NOT NULL,
    CreatedAt TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS History(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserLogin TEXT NOT NULL,
    Action TEXT NOT NULL,
    Info TEXT,
    CreatedAt TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS LoginLock(
    Login TEXT PRIMARY KEY,
    FailCount INTEGER NOT NULL,
    LockedUntil TEXT
);";
            cmd.ExecuteNonQuery();
        }
    }
}