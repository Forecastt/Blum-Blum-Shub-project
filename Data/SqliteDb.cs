using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Blum_Blum_Shub_project.Data
{
    public sealed class SqliteDb
    {
        private readonly string _dbPath;
        private readonly string _cs;

        public SqliteDb()
        {
            _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BBS.db");
            _cs = new SqliteConnectionStringBuilder { DataSource = _dbPath }.ToString();
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

-- История действий (регистрация/вход/выход/генерация/ошибки)
CREATE TABLE IF NOT EXISTS History(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserLogin TEXT NOT NULL,
    Action TEXT NOT NULL,
    Info TEXT,
    CreatedAt TEXT NOT NULL
);

-- Таблица блокировок по логину
CREATE TABLE IF NOT EXISTS LoginLock(
    Login TEXT PRIMARY KEY,
    FailCount INTEGER NOT NULL,
    LockedUntil TEXT
);";
            cmd.ExecuteNonQuery();
        }
    }
}
