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

        public Window Window { get; init; }
        public Process Process { get; init; }
        public bool OnCurrentVirtualDesktop { get; init; }

        public WindowDetails(Window window)
        {
            Window = window;
            Process = Process.GetProcessById(Window.ProcessId);
            OnCurrentVirtualDesktop = VirtualDesktopHelper.IsCurrentVirtualDesktop(window.hWnd);
        }

        public override bool Equals(object? obj)
        {
            var wd = obj as WindowDetails;
            if (wd == null)
                return false;

            return wd.Window.Equals(this);
        }

        public override int GetHashCode()
        {
            return Window.GetHashCode();
        }
    }
}
