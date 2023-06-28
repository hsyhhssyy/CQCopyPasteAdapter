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

namespace CQCopyPasteAdapter.Dialogs
{
    /// <summary>
    /// InputBox.xaml 的交互逻辑
    /// </summary>
    public partial class InputBox : Window
    {
        public InputBox()
        {
            InitializeComponent();
        }

        public string Answer { get; set; }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Answer = inputTextBox.Text;
            this.DialogResult = true;
            this.Close();
        }
    }
}
