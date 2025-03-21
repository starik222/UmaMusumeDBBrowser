﻿using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Collections;

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
            try
            {
                IntPtr hdcSrc = User32.GetWindowDC(IntPtr.Zero);
                if (hdcSrc == IntPtr.Zero)
                    return null;
                User32.RECT windowRect = new User32.RECT();
                User32.GetClientRect(handle, ref windowRect);
                User32.MapWindowPoints(handle, IntPtr.Zero, ref windowRect, 2);
                int width = windowRect.Right - windowRect.Left;
                int height = windowRect.Bottom - windowRect.Top;
                if (width <= 0 || height <= 0)
                {
                    User32.ReleaseDC(handle, hdcSrc);
                    return null;
                }
                IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
                IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
                if (hBitmap == IntPtr.Zero)
                {
                    GDI32.DeleteDC(hdcDest);
                    User32.ReleaseDC(handle, hdcSrc);
                    return null;
                }
                IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
                GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, windowRect.Left, windowRect.Top, GDI32.SRCCOPY);
                GDI32.SelectObject(hdcDest, hOld);
                GDI32.DeleteDC(hdcDest);
                User32.ReleaseDC(handle, hdcSrc);
                Image img = Image.FromHbitmap(hBitmap);
                GDI32.DeleteObject(hBitmap);
                return img;
            }
            catch (Exception)
            {
                return null;
            }
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

        public static DataTable GetProcessesTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("pHandle", typeof(IntPtr));
            dataTable.Columns.Add("pIcon", typeof(Image));
            dataTable.Columns.Add("ProcessName", typeof(string));
            dataTable.Columns.Add("WindowTitle", typeof(string));
            foreach (Process pList in Process.GetProcesses())
            {
                IntPtr handle = pList.MainWindowHandle;
                if (handle == IntPtr.Zero)
                    continue;
                Image icon = GetSmallWindowIcon(handle);
                string pName = pList.ProcessName;
                string pWindowTitle = pList.MainWindowTitle;
                dataTable.Rows.Add(handle, icon, pName, pWindowTitle);
            }
            return dataTable;
        }

        /// <summary>
        /// 64 bit version maybe loses significant 64-bit specific information
        /// </summary>
        static IntPtr GetClassLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
                return new IntPtr((long)User32.GetClassLong32(hWnd, nIndex));
            else
                return User32.GetClassLong64(hWnd, nIndex);
        }


        private static uint WM_GETICON = 0x007f;
        private static IntPtr ICON_SMALL2 = new IntPtr(2);
        private static IntPtr IDI_APPLICATION = new IntPtr(0x7F00);
        private static int GCL_HICON = -14;

        public static Image GetSmallWindowIcon(IntPtr hWnd)
        {
            try
            {
                IntPtr hIcon = default(IntPtr);

                hIcon = User32.SendMessage(hWnd, WM_GETICON, ICON_SMALL2, IntPtr.Zero);

                if (hIcon == IntPtr.Zero)
                    hIcon = GetClassLongPtr(hWnd, GCL_HICON);

                if (hIcon == IntPtr.Zero)
                    hIcon = User32.LoadIcon(IntPtr.Zero, (IntPtr)0x7F00/*IDI_APPLICATION*/);

                if (hIcon != IntPtr.Zero)
                    return new Bitmap(Icon.FromHandle(hIcon).ToBitmap(), 16, 16);
                else
                    return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<IntPtr> GetChildWindows(IntPtr parent)
        {
            List<IntPtr> result = new List<IntPtr>();
            GCHandle listHandle = GCHandle.Alloc(result);
            try
            {
                User32.EnumWindowProc childProc = new User32.EnumWindowProc(EnumWindow);
                User32.EnumChildWindows(parent, childProc, GCHandle.ToIntPtr(listHandle));
            }
            finally
            {
                if (listHandle.IsAllocated)
                    listHandle.Free();
            }
            return result;
        }

        private static bool EnumWindow(IntPtr handle, IntPtr pointer)
        {
            GCHandle gch = GCHandle.FromIntPtr(pointer);
            List<IntPtr> list = gch.Target as List<IntPtr>;
            if (list == null)
            {
                throw new InvalidCastException("GCHandle Target could not be cast as List<IntPtr>");
            }
            list.Add(handle);
            return true;
        }

        public static User32.WINDOWINFO GetWindowInfo(IntPtr handle)
        {
            User32.WINDOWINFO info = new User32.WINDOWINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            User32.GetWindowInfo(handle, ref info);
            return info;
        }

        public static string GetWindowTextRaw(IntPtr hwnd)
        {
            // Allocate correct string length first
            int length = (int)User32.SendMessage(hwnd, User32.WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);
            StringBuilder sb = new StringBuilder(length + 1);
            User32.SendMessage(hwnd, User32.WM_GETTEXT, (IntPtr)sb.Capacity, sb);
            return sb.ToString();
        }

        public static ArrayList GetAllWindows()
        {
            var windowHandles = new ArrayList();
            User32.EnumedWindow callBackPtr = GetWindowHandle;
            User32.EnumWindows(callBackPtr, windowHandles);

            //foreach (IntPtr windowHandle in windowHandles.ToArray())
            //{
            //    User32.EnumChildWindows(windowHandle, callBackPtr, windowHandles);
            //}

            return windowHandles;
        }

        private static bool GetWindowHandle(IntPtr windowHandle, ArrayList windowHandles)
        {
            windowHandles.Add(windowHandle);
            return true;
        }


    }
}