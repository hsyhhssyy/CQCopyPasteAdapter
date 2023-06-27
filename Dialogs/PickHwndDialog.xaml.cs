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
        public IntPtr HWND { get; private set; }
        public string Title { get; private set; }

        public PickHwndDialog()
        {
            InitializeComponent();
        }

        private async void PickButton_Click(object sender, RoutedEventArgs e)
        {
            PickButton.IsEnabled = false;
            await Task.Delay(3000);
            HWND = WindowHelper.GetActiveWindowHandle();
            Title = WindowHelper.GetActiveWindowTitle();
            DialogResult = true;
        }
    }
}
