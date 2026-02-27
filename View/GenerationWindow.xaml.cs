using Blum_Blum_Shub_project.Core;
using Blum_Blum_Shub_project.Data;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Blum_Blum_Shub_project.View
{
    public partial class GenerationWindow : Window
    {
        private readonly SessionManager _session = new SessionManager();
        private readonly SqliteDb _db;
        private readonly string _login;

        private readonly HistoryRepository _history;

        public GenerationWindow(string login, SqliteDb db)
        {
            InitializeComponent();
            _login = login;
            _db = db;
            _history = new HistoryRepository(db);

            HeaderText.Text = $"Генерация чисел (пользователь: {_login})";

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

        private bool TryParseInputs(out int n, out double x0, out string error)
        {
            error = "";
            n = 0;
            x0 = 0;

            if (!int.TryParse(NBox.Text.Trim(), out n) || n <= 0 || n > 2000)
            {
                error = "N должно быть целым числом (1..2000).";
                return false;
            }

            var raw = X0Box.Text.Trim().Replace(',', '.');
            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out x0))
            {
                error = "X0 должно быть числом.";
                return false;
            }

            return true;
        }

        private void Gen_Click(object sender, RoutedEventArgs e)
        {
            if (!TryParseInputs(out int n, out double x0, out string err))
            {
                MessageBox.Show(err, "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var gen = new Generation { N = n, X0 = x0 };
                gen.GenerationPrimeNumber();
                gen.GenerationNumber();

                OutputBox.Text =
                    "=== Простые p (p mod 4 = 3) ===\n" +
                    string.Join(" ", gen.PrimeNumber) +
                    "\n\n=== Сгенерированные числа ===\n" +
                    string.Join(" ", gen.Numbers);

                _history.Add(_login, "GENERATE", $"N={n}; X0={x0.ToString(CultureInfo.InvariantCulture)}");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка генерации", ex);
                MessageBox.Show("Ошибка при выполнении генерации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow(_login, _db).Show();
            Close();
        }

        private static readonly Regex DigitsOnly = new Regex(@"^\d+$");

        private void NBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !DigitsOnly.IsMatch(e.Text);
        }

        private void X0Box_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\d\.,]");
        }
    }
}