using cswm.Win32;

namespace cswm.TaskBarApp
{
    public class TaskBarApp : ApplicationContext
    {
        private NotifyIcon notifyIcon = new NotifyIcon();
        private WinEventListener eventListener;
        private WindowManager windowManager;
        private WinEventTraceFile traceFile;

        public TaskBarApp(bool traceEnabled = false)
        {            
            notifyIcon.ContextMenuStrip = BuildNotificationContextMenu();
            notifyIcon.Text = Properties.Resources.TrayIconTooltip;
            notifyIcon.Icon = Properties.Resources.Icon;
            notifyIcon.Click += NotifyIcon_OnClick;
            notifyIcon.Visible = true;

            eventListener = new WinEventListener();
            var strategy = new BSPTilingStrategy
            {
                MonitorPadding = Properties.Settings.Default.DisplayPadding,
                WindowMargin = Properties.Settings.Default.WindowMargin,
                AspectRatio = Properties.Settings.Default.AspectRatio,
            };
            windowManager = new WindowManager(eventListener, strategy);

            if (traceEnabled)
            {
                traceFile = new WinEventTraceFile(eventListener);
            }
        }

        private ContextMenuStrip BuildNotificationContextMenu()
        {
            var exitMenuItem = new ToolStripMenuItem("Exit", null, Exit_OnClick);

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(exitMenuItem);

            return contextMenu;
        }

        private void NotifyIcon_OnClick(object? sender, EventArgs e)
        {
            windowManager.Tracker.RefreshTrackedWindows();
        }

        private void Exit_OnClick(object? sender, EventArgs e)
        {
            windowManager?.Dispose();
            traceFile?.Dispose();
            eventListener?.Dispose();

            notifyIcon.Visible = false;
            Application.Exit();
        }
    }
}
