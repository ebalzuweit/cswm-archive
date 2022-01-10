using PInvoke;

namespace cswm
{
    public record WindowLayout
    {
        public IntPtr hWnd { get; init; }
        public RECT Rect { get; init; }
        public int X => Rect.left;
        public int Y => Rect.top;
        public int Width => Rect.right - Rect.left;
        public int Height => Rect.bottom - Rect.top;
    }
}
