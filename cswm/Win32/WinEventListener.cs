using PInvoke;

namespace cswm.Win32
{
    public class WinEventListener : IDisposable
    {

        public delegate void WinEventDelegate(WinEvent winEvent);

        public WinEventDelegate EventRaised;

        private User32.WinEventProc winEventProc;
        private User32.SafeEventHookHandle eventHookHandle;

        public WinEventListener() : this(User32.WindowsEventHookType.EVENT_MIN, User32.WindowsEventHookType.EVENT_MAX) { }
        public WinEventListener(User32.WindowsEventHookType eventMin, User32.WindowsEventHookType eventMax)
        {
            winEventProc = new User32.WinEventProc(WinEventRaised);
            eventHookHandle = User32.SetWinEventHook
            (
                eventMin, 
                eventMax, 
                IntPtr.Zero, 
                winEventProc, 
                0, 
                0, 
                User32.WindowsEventHookFlags.WINEVENT_OUTOFCONTEXT | User32.WindowsEventHookFlags.WINEVENT_SKIPOWNPROCESS
            );
        }

        public void Dispose()
        {
            eventHookHandle.Dispose();
            eventHookHandle = null;
        }

        private void WinEventRaised(IntPtr hWinEventHook, User32.WindowsEventHookType eventType, IntPtr hWnd, int idObject, int idChild, int dwEventThread, uint dwmsEventTime)
        {
            var winEvent = new WinEvent
            {
                hWinEventHook = hWinEventHook,
                eventType = eventType,
                hWnd = hWnd,
                idObject = idObject,
                idChild = idChild,
                dwEventThread = dwEventThread,
                dwmsEventTime = dwmsEventTime
            };

            EventRaised?.Invoke(winEvent);
        }

        public sealed record WinEvent
        {
            public IntPtr hWinEventHook { get; init; }
            public User32.WindowsEventHookType eventType { get; init; }
            public IntPtr hWnd { get; init; }
            public int idObject { get; init; }
            public int idChild { get; init; }
            public int dwEventThread { get; init; }
            public uint dwmsEventTime { get; init; }
        }
    }
}
