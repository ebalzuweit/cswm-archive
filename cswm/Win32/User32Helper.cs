using PInvoke;
using System.Runtime.InteropServices;

namespace cswm.Win32
{
    internal static class User32Helper
    {
        public static List<IntPtr> EnumWindowHandles()
        {
            var handles = new List<IntPtr>();
            User32.EnumWindows((hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);

            return handles;
        }

        public static string GetWindowTitle(IntPtr hWnd)
        {
            var len = User32.GetWindowTextLength(hWnd);
            var title = new char[len];
            User32.GetWindowText(hWnd, title, len);

            return new string(title);
        }

        public static User32.WINDOWINFO GetWindowInfo(IntPtr hWnd)
        {
            User32.WINDOWINFO info = new User32.WINDOWINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(User32.WINDOWINFO)),
            };
            if (User32.GetWindowInfo(hWnd, ref info))
            {
                return info;
            }
            return new User32.WINDOWINFO();
        }

        public static User32.MONITORINFO GetMonitorInfo(IntPtr hMonitor)
        {
            User32.MONITORINFO info = new User32.MONITORINFO
            {
                cbSize = Marshal.SizeOf(typeof(User32.MONITORINFO)),
            };
            if (User32.GetMonitorInfo(hMonitor, ref info))
            {
                return info;
            }
            return new User32.MONITORINFO();
        }

        public static List<IntPtr> EnumDisplayMonitorHandles()
        {
            var handles = new List<IntPtr>();
            bool callback(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
            {
                handles.Add(hMonitor);
                return true;
            }
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);

            return handles;
        }

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    }
}
