using System;
using System.Threading.Tasks;
using System.Windows;
using Blum_Blum_Shub_project.Data;
using Blum_Blum_Shub_project.View;
using Microsoft.Data.Sqlite;

namespace Blum_Blum_Shub_project
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 1) Глобальные обработчики исключений (на всю программу)
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            base.OnStartup(e);

            try
            {
                var db = new SqliteDb();
                db.EnsureCreated(); // создаст BBS.db + Logs + таблицы
                new LoginWindow(db).Show();
            }
            catch (Exception ex)
            {
                Logger.Error("CRITICAL_STARTUP", ex);
                MessageBox.Show("Система временно недоступна.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            HandleGlobalException(e.Exception);
            e.Handled = true; // чтобы не падало с крашем
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                HandleGlobalException(ex);
            else
                Logger.Error("CRITICAL_UNKNOWN", null);

            // тут нельзя гарантированно “продолжить”, но лог успеет записаться
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            HandleGlobalException(e.Exception);
            e.SetObserved();
        }

        private static void HandleGlobalException(Exception ex)
        {
            // Логируем детали — пользователю показываем общее сообщение (по ТЗ)
            Logger.Error("UNHANDLED", ex);

            string msg =
                ex is FormatException ? "Некорректные данные." :
                ex is SqliteException ? "Система временно недоступна." :
                ex is UnauthorizedAccessException ? "Недостаточно прав для записи файлов приложения." :
                "Произошла ошибка. Попробуйте повторить действие.";

            MessageBox.Show(msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}