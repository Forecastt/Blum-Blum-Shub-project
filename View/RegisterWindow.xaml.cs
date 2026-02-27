using Blum_Blum_Shub_project.Data;
using System.Text.RegularExpressions;
using System.Windows;

namespace Blum_Blum_Shub_project.View
{
    public partial class RegisterWindow : Window
    {
        private readonly SqliteDb _db;
        private readonly UserRepository _users;
        private readonly HistoryRepository _history;

        public RegisterWindow(SqliteDb db)
        {
            InitializeComponent();
            _db = db;
            _users = new UserRepository(db);
            _history = new HistoryRepository(db);
        }

        private bool Validate(string login, string pass, out string error)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(pass))
            {
                error = "Заполните логин и пароль.";
                return false;
            }

            if (login.Length < 3 || login.Length > 20)
            {
                error = "Логин: 3–20 символов.";
                return false;
            }

            if (pass.Length < 6 ||
                !Regex.IsMatch(pass, @"[A-Za-zА-Яа-я]") ||
                !Regex.IsMatch(pass, @"\d"))
            {
                error = "Пароль слабый: минимум 6 символов, буква и цифра.";
                return false;
            }

            return true;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string pass = PasswordBox.Password;

            if (!Validate(login, pass, out var err))
            {
                StatusText.Text = err;
                return;
            }

            try
            {
                if (_users.Register(login, pass, out var dbErr))
                {
                    _history.Add(login, "REGISTER_OK");
                    MessageBox.Show("Аккаунт был создан");
                    new LoginWindow(_db).Show();
                    Close();
                }
                else
                {
                    _history.Add(login, "REGISTER_FAIL", dbErr);
                    StatusText.Text = string.IsNullOrWhiteSpace(dbErr) ? "Аккаунт не был создан" : dbErr;
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error("Ошибка при регистрации", ex);
                StatusText.Text = "Ошибка при работе с БД.";
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow(_db).Show();
            Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}