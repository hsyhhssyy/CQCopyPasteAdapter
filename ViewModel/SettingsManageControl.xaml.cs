using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace CQCopyPasteAdapter.ViewModel
{
    public class KeyValuePairModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _key;
        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged(nameof(Key));
            }
        }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
                App.Settings[Key] = _value;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// SettingsManageControl.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsManageControl : UserControl
    {
        public ObservableCollection<KeyValuePairModel> Settings { get; set; }

        public SettingsManageControl()
        {
            Settings = new ObservableCollection<KeyValuePairModel>(App.Settings.Select(kv => new KeyValuePairModel { Key = kv.Key, Value = kv.Value }));

            InitializeComponent();

            dgrSettings.ItemsSource = Settings;
        }
    }
}
