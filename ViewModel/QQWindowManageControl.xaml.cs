using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
using CQCopyPasteAdapter.Dialogs;
using CQCopyPasteAdapter.Helpers;
using CQCopyPasteAdapter.Storage;

namespace CQCopyPasteAdapter.ViewModel
{
    /// <summary>
    /// QQWindowManageControl.xaml 的交互逻辑
    /// </summary>
    public partial class QQWindowManageControl : UserControl
    {
        public QQWindowManageControl()
        {
            InitializeComponent();


            foreach (var qqWindow in App.QQWindows)
            {
                var item = new MultiPropertyListViewItem();
                item.Data = qqWindow.Value;
                item.Properties["ChannelId"] = qqWindow.Key;
                item.Properties["HWND"] = qqWindow.Value.GetValueOrDefault("HWND") ?? "";
                item.Properties["Title"] = qqWindow.Value.GetValueOrDefault("Title") ?? "";
                QQWindows.Add(item);
            }

            grdQQWindows.ItemsSource = QQWindows;
        }

        private ObservableCollection<MultiPropertyListViewItem> QQWindows { get; } =
            new ObservableCollection<MultiPropertyListViewItem>();

        private void BtnPickHwnd_OnClick(object sender, RoutedEventArgs e)
        {
            var data = (MultiPropertyListViewItem)((Button)e.Source).DataContext;

            var wind = new PickHwndDialog();
            wind.ShowDialog();

            if (wind.HWND != IntPtr.Zero)
            {
                var dict = data.Data as NotifiedDictionary<String, String>;
                dict["HWND"] = wind.HWND.ToString();
                dict["Title"]=wind.Title;
                data.Properties["HWND"] = dict.GetValueOrDefault("HWND") ?? "";
                data.Properties["Title"] = dict.GetValueOrDefault("Title") ?? "";

                grdQQWindows.ItemsSource = null;
                grdQQWindows.ItemsSource = QQWindows;
            }


        }
    }
}
