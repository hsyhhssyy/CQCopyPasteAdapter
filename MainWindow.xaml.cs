using System;
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
                txtLogs.Text += Environment.NewLine + e.Message;
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
    }
}
