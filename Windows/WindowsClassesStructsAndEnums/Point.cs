using System.Runtime.InteropServices;

namespace OgeApp.Windows.WindowsClassesStructsAndEnums
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;

        public int y;

        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
