using System;
using System.Reflection;
using System.Windows;
using CQCopyPasteAdapter.Logging;

namespace CQCopyPasteAdapter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            App.PublicDispatcher = Dispatcher;

            Logger.Current.OnLog += CurrentLogger_OnLog;
        }

        private void CurrentLogger_OnLog(object? sender, Logger.LogReceivedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                txtLogs.Text += $"{Environment.NewLine}[{DateTime.Now:S}]{e.Message}";
                txtLogs.ScrollToEnd();
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var hc = new HookChecker();
            hc.Show();
        }

        private void BtnMessageTester_Click(object sender, RoutedEventArgs e)
        {
            var hc = new MessageTester();
            hc.Show();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            Title = Title + $"[Version{Assembly.GetAssembly(this.GetType())?.GetName().Version}]";
        }
    }
}
