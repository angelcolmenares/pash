namespace System.Management.Automation
{
    using System;
    using System.Runtime.InteropServices;

    internal static class ConsoleVisibility
    {
        private static bool _alwaysCaptureApplicationIO;
        internal const int SW_FORCEMINIMIZE = 11;
        internal const int SW_HIDE = 0;
        internal const int SW_MAX = 11;
        internal const int SW_MAXIMIZE = 3;
        internal const int SW_MINIMIZE = 6;
        internal const int SW_NORMAL = 1;
        internal const int SW_RESTORE = 9;
        internal const int SW_SHOW = 5;
        internal const int SW_SHOWDEFAULT = 10;
        internal const int SW_SHOWMAXIMIZED = 3;
        internal const int SW_SHOWMINIMIZED = 2;
        internal const int SW_SHOWMINNOACTIVE = 7;
        internal const int SW_SHOWNA = 8;
        internal const int SW_SHOWNOACTIVATE = 4;
        internal const int SW_SHOWNORMAL = 1;

        internal static bool AllocateHiddenConsole()
        {
            bool flag;
            if (GetConsoleWindow() != IntPtr.Zero)
            {
                return false;
            }
            IntPtr foregroundWindow = GetForegroundWindow();
            AllocConsole();
            IntPtr consoleWindow = GetConsoleWindow();
            if (consoleWindow == IntPtr.Zero)
            {
                flag = false;
            }
            else
            {
                flag = true;
                ShowWindow(consoleWindow, 0);
                AlwaysCaptureApplicationIO = true;
            }
            if ((foregroundWindow != IntPtr.Zero) && (GetForegroundWindow() != foregroundWindow))
            {
                SetForegroundWindow(foregroundWindow);
            }
            return flag;
        }

        [DllImport("kernel32.dll", SetLastError=true)]
        internal static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        internal static extern int GetConsoleProcessList([In, Out] int[] lpdwProcessList, int dwProcessCount);
        [DllImport("Kernel32.dll")]
        internal static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        public static void Hide()
        {
            IntPtr consoleWindow = GetConsoleWindow();
            if (consoleWindow == IntPtr.Zero)
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
            ShowWindow(consoleWindow, 0);
            AlwaysCaptureApplicationIO = true;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public static void Show()
        {
            IntPtr consoleWindow = GetConsoleWindow();
            if (consoleWindow == IntPtr.Zero)
            {
                throw PSTraceSource.NewInvalidOperationException();
            }
            ShowWindow(consoleWindow, 5);
            AlwaysCaptureApplicationIO = false;
        }

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static bool AlwaysCaptureApplicationIO
        {
            get
            {
                return _alwaysCaptureApplicationIO;
            }
            set
            {
                _alwaysCaptureApplicationIO = value;
            }
        }
    }
}

