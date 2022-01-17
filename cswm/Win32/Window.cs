using PInvoke;

namespace cswm.Win32
{
    public class Window
    {
        private int _processId;

        public IntPtr hWnd { get; init; }
        public long Styles { get; private set; }
        public int ProcessId { get => _processId; set => _processId = value; }
        public int ThreadId { get; private set; }
        public RECT Size { get; private set; }

        public Window(IntPtr hWnd)
        {
            this.hWnd = hWnd;
            RefreshCachedProperties();
        }

        public void RefreshCachedProperties()
        {
            Styles = User32.GetWindowLong(hWnd, User32.WindowLongIndexFlags.GWL_STYLE);
            ThreadId = User32.GetWindowThreadProcessId(hWnd, out _processId);
            if (User32.GetWindowRect(hWnd, out var rect))
            {
                Size = rect;
            }
        }

        public bool HasStyle(long style)
        {            
            return (Styles & style) != 0;
        }

        private static IDictionary<string, uint> STYLE_MAP = new Dictionary<string, uint>()
        {
            { nameof(User32.WindowStyles.WS_CAPTION), (uint)User32.WindowStyles.WS_CAPTION },
            { nameof(User32.WindowStyles.WS_CHILD), (uint)User32.WindowStyles.WS_CHILD },
            { nameof(User32.WindowStyles.WS_DISABLED), (uint)User32.WindowStyles.WS_DISABLED },
            { nameof(User32.WindowStyles.WS_MAXIMIZEBOX), (uint)User32.WindowStyles.WS_MAXIMIZEBOX },
            { nameof(User32.WindowStyles.WS_MINIMIZEBOX), (uint)User32.WindowStyles.WS_MINIMIZEBOX },
            { nameof(User32.WindowStyles.WS_OVERLAPPED), (uint)User32.WindowStyles.WS_OVERLAPPED },
            { nameof(User32.WindowStyles.WS_OVERLAPPEDWINDOW), (uint)User32.WindowStyles.WS_OVERLAPPEDWINDOW },
            { nameof(User32.WindowStyles.WS_POPUP), (uint)User32.WindowStyles.WS_POPUP },
            { nameof(User32.WindowStyles.WS_SYSMENU), (uint)User32.WindowStyles.WS_SYSMENU },
            { nameof(User32.WindowStyles.WS_VISIBLE), (uint)User32.WindowStyles.WS_VISIBLE },
        };

        private static IDictionary<string, uint> EX_STYLE_MAP = new Dictionary<string, uint>()
        {
            { nameof(User32.WindowStylesEx.WS_EX_APPWINDOW), (uint)User32.WindowStylesEx.WS_EX_APPWINDOW },
            { nameof(User32.WindowStylesEx.WS_EX_TOPMOST), (uint)User32.WindowStylesEx.WS_EX_TOPMOST },
            { nameof(User32.WindowStylesEx.WS_EX_CLIENTEDGE), (uint)User32.WindowStylesEx.WS_EX_CLIENTEDGE },
            { nameof(User32.WindowStylesEx.WS_EX_STATICEDGE), (uint)User32.WindowStylesEx.WS_EX_STATICEDGE },
            { nameof(User32.WindowStylesEx.WS_EX_WINDOWEDGE), (uint)User32.WindowStylesEx.WS_EX_WINDOWEDGE },
        };

        public IEnumerable<string> EnumerateStyles() => CheckStyleMap(STYLE_MAP);
        public IEnumerable<string> EnumerateExStyles() => CheckStyleMap(EX_STYLE_MAP);

        public override string ToString()
        {
            return hWnd.ToString();
        }

        public override bool Equals(object? obj)
        {
            var o = obj as Window;

            if (o == null)
            {
                return false;
            }

            return hWnd.Equals(o.hWnd);
        }

        public override int GetHashCode()
        {
            return hWnd.GetHashCode();
        }

        private IEnumerable<string> CheckStyleMap(IDictionary<string, uint> styleMap)
        {
            var styles = new List<string>();
            foreach (var windowStyle in styleMap)
            {
                if (HasStyle(windowStyle.Value))
                {
                    styles.Add(windowStyle.Key);
                }
            }
            return styles;
        }
    }
}
