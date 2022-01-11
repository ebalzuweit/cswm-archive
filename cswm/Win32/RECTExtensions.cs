using PInvoke;

namespace cswm.Win32
{
    public static class RECTExtensions
    {
        public static int Width(this RECT rect) => rect.right - rect.left;
        public static int Height(this RECT rect) => rect.bottom - rect.top;

        public static RECT Inset(this RECT rect, int l, int t, int r, int b)
        {
            return new RECT
            {
                left = rect.left + l,
                top = rect.top + t,
                right = rect.right - r,
                bottom = rect.bottom - b,
            };
        }

        public static RECT Inset(this RECT rect, int w, int h) => Inset(rect, w, h, w, h);
        public static RECT Inset(this RECT rect, int m) => Inset(rect, m, m, m, m);

        public static bool Overlaps(this RECT rect, RECT other)
        {
            if (rect.left >= other.right || rect.top >= other.bottom || rect.right <= other.left || rect.bottom <= other.top)
                return false;
            return true;
        }

        public static int GetOverlapArea(this RECT rect, RECT other)
        {
            return Math.Min(rect.right - other.left, other.right - rect.left) * Math.Min(rect.bottom - other.top, other.bottom - rect.top);
        }
    }
}
