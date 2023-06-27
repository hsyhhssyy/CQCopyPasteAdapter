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
using System.Windows.Shapes;
using CQCopyPasteAdapter.Helpers;

namespace CQCopyPasteAdapter
{
    /// <summary>
    /// MessageTester.xaml 的交互逻辑
    /// </summary>
    public partial class MessageTester : Window
    {
        public MessageTester()
        {
            InitializeComponent();
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedChannelId = QQWindowsComboBox.Text;
            var message = MessageTextBox.Text;

            var hwndStr = App.QQWindows[selectedChannelId]["HWND"];
            if (!String.IsNullOrWhiteSpace(hwndStr))
            {
                var hwnd = IntPtr.Parse(hwndStr);
                WindowHelper.BringWindowToFront(hwnd);
                Clipboard.SetText(message);
                WindowHelper.SendCtrlV();
                Task.Delay(500).Wait();
                WindowHelper.SendCtrlEnter();
            }
        }
    }
}
