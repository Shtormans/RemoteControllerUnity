using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;

public class ScreenCapture
{
    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowDC(IntPtr hwnd);

    [DllImport("user32.dll")]
    public static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

    private enum DeviceCap
    {
        Desktopvertres = 117,
        Desktophorzres = 118
    }

    private static Size _size;

    static ScreenCapture()
    {
        _size = GetPhysicalDisplaySize();
    }

    public static Size GetPhysicalDisplaySize()
    {
        Graphics g = Graphics.FromHwnd(IntPtr.Zero);
        IntPtr desktop = g.GetHdc();

        int physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.Desktopvertres);
        int physicalScreenWidth = GetDeviceCaps(desktop, (int)DeviceCap.Desktophorzres);

        return new Size(physicalScreenWidth, physicalScreenHeight);
    }

    public static byte[] CaptureScreen()
    {
        IntPtr desktopWindow = GetDesktopWindow();
        IntPtr desktopDC = GetWindowDC(desktopWindow);
        IntPtr compatibleDC = CreateCompatibleDC(desktopDC);
        IntPtr compatibleBitmap = CreateCompatibleBitmap(desktopDC, _size.Width, _size.Height);
        IntPtr oldBitmap = SelectObject(compatibleDC, compatibleBitmap);

        BitBlt(compatibleDC, 0, 0, _size.Width, _size.Height, desktopDC, 0, 0, 0x00CC0020 /* SRCCOPY */);
        //BitBlt(compatibleDC, 0, 0, 300, 300, desktopDC, 0, 0, 0x00CC0020 /* SRCCOPY */);

        Bitmap screenshot = Image.FromHbitmap(compatibleBitmap);
        //screenshot = RescaleBitmap(screenshot, 1300, 1000);

        byte[] bytes;

        using (var memoryStream = new MemoryStream())
        {
            screenshot.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            bytes = memoryStream.ToArray();
        }

        SelectObject(compatibleDC, oldBitmap);
        DeleteObject(compatibleBitmap);
        DeleteDC(compatibleDC);
        ReleaseDC(desktopWindow, desktopDC);

        return bytes;
    }

    private static Bitmap RescaleBitmap(Bitmap old, int newWidth, int newHeight)
    {
        var bmp = new Bitmap(newWidth, newHeight);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.None;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.CompositingQuality = CompositingQuality.AssumeLinear;
        g.DrawImage(old, new Rectangle(0, 0, newWidth, newHeight));
        return bmp;
    }
}