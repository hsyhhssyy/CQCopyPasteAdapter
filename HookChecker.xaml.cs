using CQCopyPasteAdapter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Threading;

namespace CQCopyPasteAdapter
{
    /// <summary>
    /// HookChecker.xaml 的交互逻辑
    /// </summary>
    public partial class HookChecker : Window
    {
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public INPUTUNION u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct INPUTUNION
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        private const uint INPUT_KEYBOARD = 1;
        private const ushort KEYEVENTF_KEYUP = 0x0002;


        private DispatcherTimer _timer;

        public HookChecker()
        {
            InitializeComponent();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            HandleTextBlock.Text = WindowHelper.GetActiveWindowHandle().ToString();
            TitleTextBlock.Text = WindowHelper.GetActiveWindowTitle();
            TextBoxHandleTextBlock.Text = TextBoxHelper.GetActiveTextBoxHandle().ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText("12345");
            SendCtrlV();
        }

        private async void SendCtrlV()
        {
            INPUT[] inputs = new INPUT[4];

            // Press Ctrl
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = 0x11; // VK_CONTROL

            // Press V
            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].u.ki.wVk = 0x56; // VK_V

            // Release V
            inputs[2].type = INPUT_KEYBOARD;
            inputs[2].u.ki.wVk = 0x56; // VK_V
            inputs[2].u.ki.dwFlags = KEYEVENTF_KEYUP;

            // Release Ctrl
            inputs[3].type = INPUT_KEYBOARD;
            inputs[3].u.ki.wVk = 0x11; // VK_CONTROL
            inputs[3].u.ki.dwFlags = KEYEVENTF_KEYUP;

            await Task.Delay(1000);

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}
