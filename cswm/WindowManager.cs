using WindowsDesktop;
using PInvoke;
using cswm.Win32;

namespace cswm
{
    public class WindowManager : IDisposable
    {
        private WinEventListener eventListener;
        private WindowTracker tracker;
        private IWindowManagementStrategy strategy;

        public WinEventListener EventListener => eventListener;
        public WindowTracker Tracker => tracker;

        public WindowManager(IWindowManagementStrategy strategy)
        {
            this.strategy = strategy;

            eventListener = new WinEventListener();
            tracker = new WindowTracker(eventListener);
            tracker.OnTrackedWindowChanged += OnTrackedWindowMoveEnd;
            tracker.OnTrackedWindowsChanged += LayoutWindows;

            LayoutWindows();
        }

        public void Dispose()
        {
            tracker.OnTrackedWindowChanged -= OnTrackedWindowMoveEnd;
            tracker.OnTrackedWindowsChanged -= LayoutWindows;
            tracker?.Dispose();

            eventListener?.Dispose();
        }

        public void LayoutWindows()
        {
            var windows = tracker.GetTrackedWindows();
            var virtualDesktops = GroupWindowsByVirtualDesktop(windows);
            foreach (var virtualDesktopWindows in virtualDesktops)
            {
                var details = virtualDesktopWindows.Value.Select(w => new WindowDetails(w));
                var windowLayouts = strategy.Apply(details);
                
                foreach (var layout in windowLayouts)
                {
                    User32.SetWindowPos
                    (
                        layout.hWnd,
                        User32.SpecialWindowHandles.HWND_NOTOPMOST,
                        layout.X,
                        layout.Y,
                        layout.Width,
                        layout.Height,
                        User32.SetWindowPosFlags.SWP_ASYNCWINDOWPOS | User32.SetWindowPosFlags.SWP_NOACTIVATE | User32.SetWindowPosFlags.SWP_SHOWWINDOW
                    );
                }
            }
        }

        private Dictionary<Guid, List<Window>> GroupWindowsByVirtualDesktop(IEnumerable<Window> windows)
        {
            var groupedWindows = new Dictionary<Guid, List<Window>>();
            foreach (var window in windows)
            {
                var virtualDesktop = VirtualDesktop.FromHwnd(window.hWnd);
                if (virtualDesktop == null)
                    continue;

                if (!groupedWindows.ContainsKey(virtualDesktop.Id))
                {
                    groupedWindows.Add(virtualDesktop.Id, new List<Window>());
                }
                groupedWindows[virtualDesktop.Id].Add(window);
            }

            return groupedWindows;
        }

        private void OnTrackedWindowMoveEnd(IntPtr hWnd)
        {
            LayoutWindows();
        }
    }
}
