using Blum_Blum_Shub_project.Data;
using System;
using System.Windows;
using System.Windows.Threading;

namespace Blum_Blum_Shub_project.View
{
    public partial class LoginWindow : Window
    {
        private readonly SqliteDb _db;
        private readonly UserRepository _users;
        private readonly HistoryRepository _history;
        private readonly LoginLockRepository _lockRepo;

        private DateTime? _lockedUntil;
        private readonly DispatcherTimer _timer;

        public LoginWindow(SqliteDb db)
        {
            InitializeComponent();

            _db = db;
            _users = new UserRepository(db);
            _history = new HistoryRepository(db);
            _lockRepo = new LoginLockRepository(db);

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, __) => UpdateLockStatus();
            _timer.Start();
        }

        private void UpdateLockStatus()
        {
            if (_lockedUntil == null)
                return;

            if (DateTime.Now >= _lockedUntil.Value)
            {
                _lockedUntil = null;
                StatusText.Text = "";
                return;
            }

            var left = _lockedUntil.Value - DateTime.Now;
            var sec = Math.Max(1, (int)Math.Ceiling(left.TotalSeconds));

            StatusText.Text = $"Слишком много попыток. Блокировка: {sec} сек.";
        }

        private bool TryGetInputs(out string login, out string pass)
        {
            login = LoginBox.Text.Trim();
            pass = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(pass))
            {
                StatusText.Text = "Заполните логин и пароль.";
                return false;
            }

            if (login.Length < 3 || login.Length > 20)
            {
                StatusText.Text = "Логин: 3–20 символов.";
                return false;
            }

            return true;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (!TryGetInputs(out var login, out var pass))
                return;

            try
            {
                var (_, until) = _lockRepo.Get(login);
                if (until != null && DateTime.Now < until.Value)
                {
                    _lockedUntil = until;
                    UpdateLockStatus();
                    return;
                }

                bool ok = _users.TryLogin(login, pass);

                if (ok)
                {
                    _lockRepo.Reset(login);
                    _history.Add(login, "LOGIN_OK");

                    new MainWindow(login, _db).Show();
                    Close();
                    return;
                }

                _lockRepo.RegisterFail(login);
                _history.Add(login, "LOGIN_FAIL");

                var (_, until2) = _lockRepo.Get(login);
                _lockedUntil = until2;

                if (_lockedUntil != null && DateTime.Now < _lockedUntil.Value)
                {
                    UpdateLockStatus();
                    return;
                }

                StatusText.Text = "Неверный логин или пароль.";
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка при авторизации", ex);
                StatusText.Text = "Ошибка при авторизации.";
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            new RegisterWindow(_db).Show();
            Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}