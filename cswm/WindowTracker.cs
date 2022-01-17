using WindowsDesktop;
using PInvoke;
using cswm.Win32;

namespace cswm
{
    public class WindowTracker : IDisposable
    {
        static readonly User32.WindowsEventHookType[] START_TRACKING_EVENTS = new User32.WindowsEventHookType[]
        {
            User32.WindowsEventHookType.EVENT_SYSTEM_MINIMIZEEND,
            User32.WindowsEventHookType.EVENT_SYSTEM_MINIMIZESTART,
            User32.WindowsEventHookType.EVENT_OBJECT_LOCATIONCHANGE,
            User32.WindowsEventHookType.EVENT_OBJECT_CREATE,
            User32.WindowsEventHookType.EVENT_OBJECT_SHOW,
            User32.WindowsEventHookType.EVENT_OBJECT_FOCUS,
            //(User32.WindowsEventHookType) 0x8018, // EVENT_OBJECT_UNCLOAKED
            //(User32.WindowsEventHookType) 0x7FFFFF30, // Discord raises this event when restoring from the tray, not listed as an event constant
        };

        public Action OnTrackedWindowsChanged;
        public Action<IntPtr> OnTrackedWindowChanged;

        private WinEventListener winEventListener;
        private HashSet<IntPtr> trackedWindowHandles = new HashSet<IntPtr>();
        private HashSet<Window> trackedWindows = new HashSet<Window>();

        public WindowTracker(WinEventListener winEventListener)
        {
            this.winEventListener = winEventListener;
            this.winEventListener.EventRaised += WinEventRaised;

            VirtualDesktop.CurrentChanged += OnCurrentVirtualDesktopChanged;

            RefreshTrackedWindows();
        }

        public Window[] GetTrackedWindows()
        {
            return trackedWindows.ToArray();
        }

        public void Dispose()
        {
            winEventListener.EventRaised -= WinEventRaised;
            winEventListener = null;

            VirtualDesktop.CurrentChanged -= OnCurrentVirtualDesktopChanged;
        }

        public void RefreshTrackedWindows()
        {
            trackedWindowHandles.Clear();
            trackedWindows.Clear();

            var handles = User32Helper.EnumWindowHandles();
            var windows = handles.Select(h => new Window(h));

            foreach (var window in windows)
            {
                if (TrackWindow(window))
                {
                    AddTrackedWindow(window);
                }
            }
            OnTrackedWindowsChanged?.Invoke();
        }

        private void AddTrackedWindow(Window window)
        {
            trackedWindowHandles.Add(window.hWnd);
            trackedWindows.Add(window);
        }

        private void RemoveTrackedWindow(Window window)
        {
            trackedWindowHandles.Remove(window.hWnd);
            trackedWindows.Remove(window);
        }

        private void OnCurrentVirtualDesktopChanged(object sender, VirtualDesktopChangedEventArgs args)
        {
            // placeholder
        }

        private void WinEventRaised(WinEventListener.WinEvent winEvent)
        {
            if (trackedWindowHandles.Contains(winEvent.hWnd))
            {
                // window is currently being tracked
                var window = trackedWindows.Where(w => w.hWnd.Equals(winEvent.hWnd)).Single();

                // handle may no longer be valid, stop tracking if so
                bool stopTrackingWindow = true;
                try
                {
                    window.RefreshCachedProperties();
                    stopTrackingWindow = !TrackWindow(window);
                }
                catch (Win32Exception) { }

                if (stopTrackingWindow)
                {
                    RemoveTrackedWindow(window);
                    OnTrackedWindowsChanged?.Invoke();
                }
                else
                {
                    switch (winEvent.eventType)
                    {
                        case (User32.WindowsEventHookType)0x8017: // EVENT_OBJECT_CLOAKED : virtual desktop switches
                        case User32.WindowsEventHookType.EVENT_SYSTEM_MOVESIZEEND: // : user repositioning
                            OnTrackedWindowChanged?.Invoke(winEvent.hWnd);
                            break;
                    }
                }
            }
            else if (START_TRACKING_EVENTS.Contains(winEvent.eventType))
            {
                try
                {
                    var window = new Window(winEvent.hWnd);
                    if (TrackWindow(window))
                    {
                        AddTrackedWindow(window);
                        OnTrackedWindowsChanged?.Invoke();
                    }
                }
                catch (Win32Exception e)
                {
                    return;
                }
            }
            else if (winEvent.hWnd != IntPtr.Zero)
            {
                // debugging stuff
                try
                {
                    var window = new Window(winEvent.hWnd);
                    var details = new WindowDetails(window);

                    if (details.Process.ProcessName.ToLower().Contains("discord"))
                    {
                        switch (winEvent.eventType)
                        {
                            default:
                                break;
                        }
                    }
                }
                catch (Exception e) { }
            }
        }

        static readonly int[] SYSTEM_PIDS = new int[] { 0, 4 };

        static readonly uint[] REQUIRED_STYLES = new uint[]
        {
            (uint) User32.WindowStyles.WS_OVERLAPPEDWINDOW,
            (uint) User32.WindowStyles.WS_MINIMIZEBOX,
            (uint) User32.WindowStyles.WS_MAXIMIZEBOX,
            (uint) User32.WindowStyles.WS_VISIBLE,
            (uint) User32.WindowStylesEx.WS_EX_APPWINDOW,
        };

        static readonly uint[] INVALID_STYLES = new uint[]
        {
            (uint) User32.WindowStyles.WS_CHILD,
            (uint) User32.WindowStyles.WS_POPUP,
        };

        private bool TrackWindow(Window window)
        {
            if (SYSTEM_PIDS.Any(pid => window.ProcessId == pid))
            {
                return false;
            }
            if (window.Size.Area() <= 8192) // 128 * 64
            {
                return false;
            }

            if (!REQUIRED_STYLES.All(s => window.HasStyle(s)))
            {
                return false;
            }
            if (INVALID_STYLES.Any(s => window.HasStyle(s)))
            {
                return false;
            }

            var floating = !(User32.IsIconic(window.hWnd) || User32.IsZoomed(window.hWnd) || IsZoomedCustom(window.hWnd));

            return floating;
        }

        private bool IsZoomedCustom(IntPtr hWnd)
        {
            return false;

            // what is this? documentation..

            const int MIN_WINDOW_AREA = 76800; // 320x240
            const float AREA_PCT = 0.95f;

            if (User32.GetWindowRect(hWnd, out var rect))
            {
                var area = (rect.right - rect.left) * (rect.top - rect.bottom);
                if (area < MIN_WINDOW_AREA)
                {
                    return false;
                }

                var monitorHwnd = User32.MonitorFromWindow(hWnd, User32.MonitorOptions.MONITOR_DEFAULTTONEAREST);
                //var monitorInfo = new User32.MonitorInfoEx();
                //if (User32.GetMonitorInfo(monitorHwnd, monitorInfo))
                //{
                //}
            }

            return false;
        }
    }
}