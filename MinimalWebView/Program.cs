using Microsoft.Web.WebView2.Core;
using System;
using System.Drawing;
using System.Reflection;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MinimalWebView
{
    class Program
    {
        private static CoreWebView2Controller _controller;

        [STAThread]
        static int Main(string[] args)
        {
            HWND hwnd;

            unsafe
            {
                HINSTANCE hInstance = PInvoke.GetModuleHandle((char*)null);
                ushort classId;

                HBRUSH backgroundBrush = PInvoke.CreateSolidBrush(0x271811); // this is actually #111827, Windows uses BBGGRR
                if (backgroundBrush.IsNull)
                {
                    // fallback to the system background color in case it fails
                    backgroundBrush = (HBRUSH)(IntPtr)(SYS_COLOR_INDEX.COLOR_BACKGROUND + 1);
                }

                fixed (char* classNamePtr = "MinimalWebView")
                {
                    WNDCLASSW wc = new WNDCLASSW();
                    wc.lpfnWndProc = WndProc;
                    wc.lpszClassName = classNamePtr;
                    wc.hInstance = hInstance;
                    wc.hbrBackground = backgroundBrush;
                    wc.style = WNDCLASS_STYLES.CS_VREDRAW | WNDCLASS_STYLES.CS_HREDRAW;
                    classId = PInvoke.RegisterClass(wc);

                    if (classId == 0)
                    {
                        throw new Exception("class not registered");
                    }
                }

                fixed (char* windowNamePtr = $"MinimalWebView {Assembly.GetExecutingAssembly().GetName().Version}")
                {
                    hwnd = PInvoke.CreateWindowEx(
                        0,
                        (char*)classId,
                        windowNamePtr,
                        WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                        Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT,
                        new HWND(),
                        new HMENU(),
                        hInstance,
                        null);
                }
            }

            if (hwnd.Value == 0)
            {
                throw new Exception("hwnd not created");
            }

            PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_NORMAL);

            CreateCoreWebView2(hwnd);

            MSG msg;
            while (PInvoke.GetMessage(out msg, new HWND(), 0, 0))
            {
                PInvoke.TranslateMessage(msg);
                PInvoke.DispatchMessage(msg);
            }

            return (int)msg.wParam.Value;
        }

        private static LRESULT WndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
        {
            switch (msg)
            {
                case Constants.WM_SIZE:
                    OnSize(hwnd, wParam, GetLowWord(lParam.Value), GetHighWord(lParam.Value));
                    break;

                case Constants.WM_CLOSE:
                    PInvoke.PostQuitMessage(0);
                    break;
            }

            return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
        }

        private static void OnSize(HWND hwnd, WPARAM wParam, int width, int height)
        {
            if (_controller != null)
            {
                _controller.Bounds = new Rectangle(0, 0, width, height);
            }
        }

        private static async void CreateCoreWebView2(HWND hwnd)
        {
            var environment = await CoreWebView2Environment.CreateAsync(null, null, null);

            _controller = await environment.CreateCoreWebView2ControllerAsync(hwnd);

            PInvoke.GetClientRect(hwnd, out var hwndRect);

            _controller.CoreWebView2.SetVirtualHostNameToFolderMapping("minimalwebview.example", "wwwroot", CoreWebView2HostResourceAccessKind.Allow);
            _controller.Bounds = new Rectangle(0, 0, hwndRect.right, hwndRect.bottom);
            _controller.IsVisible = true;
            _controller.CoreWebView2.Navigate("https://minimalwebview.example/index.html");
        }

        private static int GetLowWord(nint value)
        {
            uint xy = (uint)value;
            int x = unchecked((short)xy);
            return x;
        }

        private static int GetHighWord(nint value)
        {
            uint xy = (uint)value;
            int y = unchecked((short)(xy >> 16));
            return y;
        }
    }
}
