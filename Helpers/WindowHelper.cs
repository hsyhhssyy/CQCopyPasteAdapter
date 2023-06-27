using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CQCopyPasteAdapter.Helpers;

public static class WindowHelper
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

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

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    public static void BringWindowToFront(IntPtr hWnd)
    {
        SetForegroundWindow(hWnd);
    }

    public static IntPtr GetActiveWindowHandle()
    {
        return GetForegroundWindow();
    }

    public static string GetActiveWindowTitle()
    {
        const int nChars = 256;
        StringBuilder Buff = new StringBuilder(nChars);
        IntPtr handle = GetForegroundWindow();

        if (GetWindowText(handle, Buff, nChars) > 0)
        {
            return Buff.ToString();
        }
        return null;
    }

    public static void SendCtrlV()
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
        
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    public static void SendCtrlEnter()
    {
        INPUT[] inputs = new INPUT[4];

        // Press Ctrl
        inputs[0].type = INPUT_KEYBOARD;
        inputs[0].u.ki.wVk = 0x11; // VK_CONTROL

        // Press Enter
        inputs[1].type = INPUT_KEYBOARD;
        inputs[1].u.ki.wVk = 0x0D; // VK_RETURN

        // Release Enter
        inputs[2].type = INPUT_KEYBOARD;
        inputs[2].u.ki.wVk = 0x0D; // VK_RETURN
        inputs[2].u.ki.dwFlags = KEYEVENTF_KEYUP;

        // Release Ctrl
        inputs[3].type = INPUT_KEYBOARD;
        inputs[3].u.ki.wVk = 0x11; // VK_CONTROL
        inputs[3].u.ki.dwFlags = KEYEVENTF_KEYUP;

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

}