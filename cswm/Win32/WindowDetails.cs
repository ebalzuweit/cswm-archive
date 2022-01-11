using System.Diagnostics;
using WindowsDesktop;
using PInvoke;

namespace cswm.Win32
{
    /// <summary>
    /// Fetch more details about a Window, cpu-intensive.
    /// </summary>
    public class WindowDetails
    {
        private int processId;

        public Window Window { get; init; }
        public RECT Size { get; init; }
        public int ProcessId => processId;
        public int ThreadId { get; init; }
        public Process Process { get; init; }
        public bool OnCurrentVirtualDesktop { get; init; }

        public WindowDetails(Window window)
        {
            Window = window;
            if (User32.GetWindowRect(window.hWnd, out var rect))
            {
                Size = rect;
            }
            ThreadId = User32.GetWindowThreadProcessId(window.hWnd, out processId);
            Process = Process.GetProcessById((int)ProcessId);
            OnCurrentVirtualDesktop = VirtualDesktopHelper.IsCurrentVirtualDesktop(window.hWnd);
        }
    }
}
