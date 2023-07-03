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

namespace CQCopyPasteAdapter.Dialogs
{
    /// <summary>
    /// PickHwndDialog.xaml 的交互逻辑
    /// </summary>
    public partial class PickHwndDialog : Window
    {
        public IntPtr Hwnd { get; private set; }
        public string? HwndTitle { get; private set; }

        public PickHwndDialog()
        {
            InitializeComponent();
        }

        private async void PickButton_Click(object sender, RoutedEventArgs e)
        {
            pickButton.IsEnabled = false;
            int countdown = 3;
            countdownLabel.Content = $"请在{countdown.ToString()}秒内切换到对应窗口";
            while (countdown > 0)
            {
                await Task.Delay(1000);  // Wait for 1 second.
                countdown--;
                countdownLabel.Content = $"请在{countdown.ToString()}秒内切换到对应窗口";
            }
            Hwnd = WindowHelper.GetActiveWindowHandle();
            HwndTitle = WindowHelper.GetActiveWindowTitle();
            DialogResult = true;
        }
    }
}
