using Blum_Blum_Shub_project.Core;
using Blum_Blum_Shub_project.Data;
using System;
using System.Windows;

namespace Blum_Blum_Shub_project.View
{
    public partial class MainWindow : Window
    {
        private readonly SqliteDb _db;
        private readonly string _login;

        private readonly HistoryRepository _history;
        private readonly SessionManager _session = new SessionManager();

        public MainWindow(string login, SqliteDb db)
        {
            InitializeComponent();

            _login = login;
            _db = db;

            _history = new HistoryRepository(db);

            UserText.Text = $"Пользователь: {_login}";

            _session.TimedOut += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    _history.Add(_login, "TIMEOUT");
                    MessageBox.Show("Сессия завершена из-за бездействия.");
                    new LoginWindow(_db).Show();
                    Close();
                });
            };

            Loaded += (_, __) => _session.Start(this);
            Closed += (_, __) => _session.Stop(this);
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            new GenerationWindow(_login, _db).Show();
            Close();
        }

        private void History_Click(object sender, RoutedEventArgs e)
        {
            new HistoryWindow(_login, _db).ShowDialog();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _history.Add(_login, "LOGOUT");
            new LoginWindow(_db).Show();
            Close();
        }
    }
}