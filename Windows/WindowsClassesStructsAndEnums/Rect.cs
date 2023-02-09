using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace OgeApp.Windows.WindowsClassesStructsAndEnums
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
        public static readonly RECT Empty = new();
        public int Width { get { return Math.Abs(right - left); } }
        public int Height { get { return bottom - top; } }
        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }
        public RECT(RECT rcSrc)
        {
            left = rcSrc.left;
            top = rcSrc.top;
            right = rcSrc.right;
            bottom = rcSrc.bottom;
        }
        public bool IsEmpty { get { return left >= right || top >= bottom; } }
        public override string ToString()
        {
            if (this == Empty) { return "RECT {Empty}"; }
            return "RECT { left : " + left + " / top : " + top + " / right : " + right + " / bottom : " + bottom + " }";
        }
        public override bool Equals(object? obj)
        {
            if (obj is not Rect) { return false; }
            return this == (RECT)obj;
        }

        public override int GetHashCode() => left.GetHashCode() + top.GetHashCode() + right.GetHashCode() + bottom.GetHashCode();

        public static bool operator ==(RECT rect1, RECT rect2) { return rect1.left == rect2.left && rect1.top == rect2.top && rect1.right == rect2.right && rect1.bottom == rect2.bottom; }

        public static bool operator !=(RECT rect1, RECT rect2) { return !(rect1 == rect2); }
    }

}
