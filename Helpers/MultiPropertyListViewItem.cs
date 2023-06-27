using CQCopyPasteAdapter.Storage;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CQCopyPasteAdapter.Helpers
{
    /*
  * MultiPropertyListViewItem
  * 版本 v1.1
  * 最后修改 2023-02-11
  *
  * v1.0 初始版本
  * v1.1 加入ObjectProperties
  */

    public class MultiPropertyListViewItem : INotifyPropertyChanged
    {
        [UsedImplicitly] public String GroupName { get; set; } = "";

        [UsedImplicitly] public Object Data { get; set; } = "";

        [UsedImplicitly] public NotifiedDictionary<String, String> Properties { get; } = new();

        [UsedImplicitly] public NotifiedDictionary<String, object> ObjectProperties { get; } = new();

        [UsedImplicitly] public static GroupDescription GroupDescription => new MultiPropertyListViewItemGroupDescription();

        public MultiPropertyListViewItem()
        {
            Properties.CollectionChanged += Properties_CollectionChanged;
            ObjectProperties.CollectionChanged += Collections_CollectionChanged;
        }

        private void Collections_CollectionChanged(object? sender, CollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ObjectProperties));
        }

        private void Properties_CollectionChanged(object? sender, CollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Properties));
        }

        [UsedImplicitly]
        public class MultiPropertyListViewItemGroupDescription : GroupDescription
        {
            public override object GroupNameFromItem(object item, int level, CultureInfo culture)
            {
                var i = item as MultiPropertyListViewItem;
                return i?.GroupName ?? "";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
