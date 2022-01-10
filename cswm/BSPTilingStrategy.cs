using cswm.Win32;
using PInvoke;
using System.Linq;

namespace cswm
{
    /// <summary>
    /// Recursively partitions the space, splitting in half each time.
    /// </summary>
    public class BSPTilingStrategy : IWindowManagementStrategy
    {
        public int MonitorPadding { get; set; }
        public int WindowMargin { get; set; }
        public float AspectRatio { get; set; }

        public IEnumerable<WindowLayout> Apply(IEnumerable<WindowDetails> windowDetails)
        {
            var layouts = new List<WindowLayout>();
            var detailsByMonitor = windowDetails.GroupBy(d => User32.MonitorFromWindow(d.Window.hWnd, User32.MonitorOptions.MONITOR_DEFAULTTONEAREST));

            foreach (var monitorDetails in detailsByMonitor)
            {
                var monitorInfo = User32Helper.GetMonitorInfo(monitorDetails.Key);
                var monitorArea = monitorInfo.rcWork.Inset(MonitorPadding);
                var monitorLayouts = PartitionRect(monitorArea, monitorDetails);
                layouts.AddRange(monitorLayouts);
            }

            return layouts;
        }

        private IEnumerable<WindowLayout> PartitionRect(RECT area, IEnumerable<WindowDetails> windowDetails)
        {
            if (windowDetails.Count() == 0)
                return new WindowLayout[0];
            if (windowDetails.Count() == 1)
            {
                var layout = new WindowLayout
                {
                    hWnd = windowDetails.First().Window.hWnd,
                    Rect = area,
                };
                return new WindowLayout[1] { layout };
            }

            var aspectRatio = (float)area.Width() / area.Height();
            var splitHorizontal = aspectRatio >= AspectRatio;

            var split = SplitRect(area, splitHorizontal);
            var ordered = windowDetails.OrderBy(d => Math.Abs(WindowOrdering(d, split.Item1, split.Item2)));

            var maxWindows = ordered.Count() / 2;
            if (ordered.Count() % 2 == 1)
                maxWindows += 1;

            var windows1 = new List<WindowDetails>();
            var windows2 = new List<WindowDetails>();
            foreach (var window in ordered)
            {
                var pref = WindowOrdering(window, split.Item1, split.Item2);
                if (pref <= 0)
                {
                    if (windows1.Count() < maxWindows)
                    {
                        windows1.Add(window);
                    }
                    else
                    {
                        windows2.Add(window);
                    }
                }
                else
                {
                    if (windows2.Count() < maxWindows)
                    {
                        windows2.Add(window);
                    }
                    else
                    {
                        windows1.Add(window);
                    }
                }
            }

            var left = PartitionRect(split.Item1, windows1);
            var right = PartitionRect(split.Item2, windows2);

            var all = new List<WindowLayout>();
            all.AddRange(left);
            all.AddRange(right);
            return all;
        }

        private Tuple<RECT, RECT> SplitRect(RECT area, bool splitHorizontally)
        {
            if (splitHorizontally)
            {
                var width = (int)Math.Round((area.Width() - (2 * WindowMargin)) * 0.5f);
                return new Tuple<RECT, RECT>
                (
                    new RECT { left = area.left, top = area.top, right = area.left + width, bottom = area.bottom },
                    new RECT { left = area.right - width, top = area.top, right = area.right, bottom = area.bottom }
                );
            }
            else
            {
                var height = (int)Math.Round((area.Height() - (2 * WindowMargin)) * 0.5f);
                return new Tuple<RECT, RECT>
                (
                    new RECT { left = area.left, top = area.top, right = area.right, bottom = area.top + height },
                    new RECT { left = area.left, top = area.bottom - height, right = area.right, bottom= area.bottom }
                );
            }
        }

        private float WindowOrdering(WindowDetails details, RECT a, RECT b)
        {
            int overlap_a = 0, overlap_b = 0;
            if (details.Size.Overlaps(a))
                overlap_a = details.Size.GetOverlapArea(a);
            if (details.Size.Overlaps(b))
                overlap_b = details.Size.GetOverlapArea(b);

            return overlap_b - overlap_a;
        }
    }
}
