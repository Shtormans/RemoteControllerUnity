using System.Drawing;
using System.Runtime.InteropServices;

public class MouseImpersonator
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

    private const int leftDown = 0x02;
    private const int leftUp = 0x04;

    [DllImport("User32.Dll")]
    public static extern long SetCursorPos(int x, int y);

    public static void Grab(int xPos, int yPos)
    {
        SetCursorPos(xPos, yPos);
    }

    public static void Press(int xPos, int yPos)
    {
        mouse_event(leftDown, (uint)xPos, (uint)yPos, 0, 0);
    }

    public static void Release(int xPos, int yPos)
    {
        mouse_event(leftUp, (uint)xPos, (uint)yPos, 0, 0);
    }
}