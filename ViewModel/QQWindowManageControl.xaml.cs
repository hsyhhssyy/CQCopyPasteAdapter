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

            if (wind.Hwnd != IntPtr.Zero)
            {
                if (data.Data is NotifiedDictionary<string, string> dict)
                {
                    dict["HWND"] = wind.Hwnd.ToString();
                    dict["Title"] = wind.HwndTitle ?? "";
                    data.Properties["HWND"] = dict.GetValueOrDefault("HWND") ?? "";
                    data.Properties["Title"] = dict.GetValueOrDefault("Title") ?? "";

                    grdQQWindows.ItemsSource = null;
                    grdQQWindows.ItemsSource = QQWindows;
                }
            }


        }

        private void BtnAddChannel_Click(object sender, RoutedEventArgs e)
        {
            InputBox inputBox = new InputBox();
            if (inputBox.ShowDialog() == true)
            {
                string answer = inputBox.Answer;

                App.QQWindows.Add(answer,new NotifiedDictionary<string, string>());

                var qqWindow = App.QQWindows[answer];

                var item = new MultiPropertyListViewItem();
                item.Data = qqWindow;
                item.Properties["ChannelId"] = answer;
                item.Properties["HWND"] = qqWindow.GetValueOrDefault("HWND") ?? "";
                item.Properties["Title"] = qqWindow.GetValueOrDefault("Title") ?? "";
                QQWindows.Add(item);

                grdQQWindows.ItemsSource = null;
                grdQQWindows.ItemsSource = QQWindows;
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            //弹出确认对话框
            MessageBoxResult result = MessageBox.Show("是否删除所选映射?", "确认", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes) //如果用户选择了“是”
            {
                //从ObservableCollection删除选中项
                if (grdQQWindows.SelectedItem is MultiPropertyListViewItem item)
                {
                    QQWindows.Remove(item);

                    //从SqliteKvStore删除相应项
                    App.QQWindows.Remove(item.Properties["ChannelId"]);
                }
            }
        }

    }
}
