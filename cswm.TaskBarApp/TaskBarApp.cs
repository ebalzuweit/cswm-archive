using cswm.Win32;

namespace cswm.TaskBarApp
{
    public class TaskBarApp : ApplicationContext
    {
        private NotifyIcon notifyIcon = new NotifyIcon();
        private WindowManager windowManager;

        public TaskBarApp()
        {            
            notifyIcon.ContextMenuStrip = BuildNotificationContextMenu();
            notifyIcon.Text = Properties.Resources.TrayIconTooltip;
            notifyIcon.Icon = Properties.Resources.Icon;
            notifyIcon.Click += NotifyIcon_OnClick;
            notifyIcon.Visible = true;

            var strategy = new BSPTilingStrategy
            {
                MonitorPadding = Properties.Settings.Default.DisplayPadding,
                WindowMargin = Properties.Settings.Default.WindowMargin,
                AspectRatio = Properties.Settings.Default.AspectRatio,
            };
            windowManager = new WindowManager(strategy);
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

            notifyIcon.Visible = false;
            Application.Exit();
        }
    }
}
