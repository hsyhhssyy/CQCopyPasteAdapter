using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace CQCopyPasteAdapter.ViewModel
{
    public class KeyValuePairModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly string? _key;
        public string? Key
        {
            get => _key;
            init
            {
                _key = value;
                OnPropertyChanged(nameof(Key));
            }
        }

        private string? _value;
        public string? Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
                if (Key != null)
                    if (_value != null)
                        App.Settings[Key] = _value;
            }
        }

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// SettingsManageControl.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsManageControl
    {
        private ObservableCollection<KeyValuePairModel> Settings { get;  }

        public SettingsManageControl()
        {
            Settings = new ObservableCollection<KeyValuePairModel>(App.Settings.Select(kv => new KeyValuePairModel { Key = kv.Key, Value = kv.Value }));

            InitializeComponent();

            dgrSettings.ItemsSource = Settings;
        }
    }
}
