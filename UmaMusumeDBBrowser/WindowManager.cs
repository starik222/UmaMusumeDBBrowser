using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace UmaMusumeDBBrowser
{
    public static class WindowManager
    {
        public static Image CaptureScreen()
        {
            return CaptureWindow(User32.GetDesktopWindow());
        }
        public static Image CaptureScreen(IntPtr hWnd)
        {
            return CaptureWindow(hWnd);
        }

        public static IntPtr FindWindow(string lpClassName, string lpWindowName)
        {
            return User32.FindWindow(lpClassName, lpWindowName);
        }
        public static Image CaptureWindow(IntPtr handle)
        {
            IntPtr hdcSrc = User32.GetWindowDC(IntPtr.Zero);
            User32.RECT windowRect = new User32.RECT();
            User32.GetClientRect(handle, ref windowRect);
            User32.MapWindowPoints(handle, IntPtr.Zero, ref windowRect, 2);
            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, windowRect.Left, windowRect.Top, GDI32.SRCCOPY);
            GDI32.SelectObject(hdcDest, hOld);
            GDI32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);
            Image img = Image.FromHbitmap(hBitmap);
            GDI32.DeleteObject(hBitmap);
            return img;
        }
        public static void CaptureWindowToFile(IntPtr handle, string filename, ImageFormat format)
        {
            Image img = CaptureWindow(handle);
            img.Save(filename, format);
        }
        public static void CaptureScreenToFile(string filename, ImageFormat format)
        {
            Image img = CaptureScreen();
            img.Save(filename, format);
        }


        public static IntPtr GetHandleByProcessName(string wName)
        {
            foreach (Process pList in Process.GetProcesses())
                if (pList.MainWindowTitle.Contains(wName))
                    return pList.MainWindowHandle;

            return IntPtr.Zero;
        }

        //public static IntPtr GetWindowByHandle(IntPtr handle)
        //{

        //}

    }
}