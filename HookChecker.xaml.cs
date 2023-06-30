using CQCopyPasteAdapter.Helpers;
using System;
using System.Windows.Threading;

namespace CQCopyPasteAdapter
{
    /// <summary>
    /// HookChecker.xaml 的交互逻辑
    /// </summary>
    public partial class HookChecker
    {
        public HookChecker()
        {
            InitializeComponent();

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            handleTextBlock.Text = WindowHelper.GetActiveWindowHandle().ToString();
            titleTextBlock.Text = WindowHelper.GetActiveWindowTitle();
            textBoxHandleTextBlock.Text = TextBoxHelper.GetActiveTextBoxHandle().ToString();
        }
        
    }
}
