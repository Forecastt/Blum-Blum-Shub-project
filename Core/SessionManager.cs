using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Blum_Blum_Shub_project.Core
{
    public sealed class SessionManager
    {
        private readonly DispatcherTimer _timer;
        private DateTime _lastActivity;

        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(10);

        public event Action? TimedOut;

        public SessionManager()
        {
            _lastActivity = DateTime.Now;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _timer.Tick += (_, __) => Check();
        }

        public void Start(Window window)
        {
            window.PreviewMouseDown += OnActivity;
            window.PreviewKeyDown += OnActivity;
            window.PreviewMouseWheel += OnActivity;
            _timer.Start();
        }

        public void Stop(Window window)
        {
            _timer.Stop();
            window.PreviewMouseDown -= OnActivity;
            window.PreviewKeyDown -= OnActivity;
            window.PreviewMouseWheel -= OnActivity;
        }

        public void Ping() => _lastActivity = DateTime.Now;

        private void OnActivity(object? sender, EventArgs e) => Ping();

        private void Check()
        {
            if (DateTime.Now - _lastActivity >= Timeout)
                TimedOut?.Invoke();
        }
    }
}