using System;
using System.Threading.Tasks;
using System.Windows;
using CQCopyPasteAdapter.Helpers;

namespace CQCopyPasteAdapter
{
    /// <summary>
    /// MessageTester.xaml 的交互逻辑
    /// </summary>
    public partial class MessageTester
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
                WindowHelper.SendCtrlA();
                Task.Delay(100).Wait();
                WindowHelper.SendDelete();
                Task.Delay(100).Wait();
                WindowHelper.SendCtrlV();
                Task.Delay(100).Wait();
                WindowHelper.SendCtrlEnter();
            }
        }
    }
}
