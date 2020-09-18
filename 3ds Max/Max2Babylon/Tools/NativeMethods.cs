﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Max2Babylon
{
    internal static class NativeMethods
    {
        internal delegate bool WindowEnumProc(IntPtr hwnd, IntPtr lparam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumWindows(WindowEnumProc callPtr, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetWindowTextLength(IntPtr hWnd);
    }
}
