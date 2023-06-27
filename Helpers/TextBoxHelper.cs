using System;
using System.Windows.Automation;

namespace CQCopyPasteAdapter.Helpers;

public static class TextBoxHelper
{
    public static IntPtr GetActiveTextBoxHandle()
    {
        AutomationElement focusedElement = AutomationElement.FocusedElement;

        if (focusedElement == null)
        {
            return IntPtr.Zero;
        }

        object valuePattern = null;
        if (focusedElement.TryGetCurrentPattern(ValuePattern.Pattern, out valuePattern))
        {
            // Element is a TextBox, return its handle
            return new IntPtr(focusedElement.Current.NativeWindowHandle);
        }

        // Element is not a TextBox
        return new IntPtr(focusedElement.Current.NativeWindowHandle);
    }
}