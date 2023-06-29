using CQCopyPasteAdapter.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CQCopyPasteAdapter.Logging;

namespace CQCopyPasteAdapter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
