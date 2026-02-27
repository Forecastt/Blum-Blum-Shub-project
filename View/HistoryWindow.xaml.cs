using System.Linq;
using System.Windows;
using Blum_Blum_Shub_project.Data;

namespace Blum_Blum_Shub_project.View
{
    public partial class HistoryWindow : Window
    {
        private readonly string _login;
        private readonly HistoryRepository _history;

        public HistoryWindow(string login, SqliteDb db)
        {
            InitializeComponent();
            _login = login;
            _history = new HistoryRepository(db);
            LoadData();
        }

        private void LoadData()
        {
            GridHistory.ItemsSource = _history.GetForUser(_login)
                .Select(x => new { x.CreatedAt, x.Action, x.Info })
                .ToList();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _history.ClearForUser(_login);
            LoadData();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}