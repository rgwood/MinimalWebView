using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace MinimalWebView
{
    class Program
    {
        private static CoreWebView2Controller _controller;
        private static bool _webView2HasBeenInitialized = false;

        [STAThread]
        static void Main(string[] args)
        {
            string className = "MinimalWebView";
            IntPtr hInstance = Process.GetCurrentProcess().Handle;

            ushort classId;

            HWND hwnd;

            unsafe
            {
                fixed (char* classNamePtr = className)
                {
                    WNDCLASSW wc = new WNDCLASSW();
                    wc.lpfnWndProc = WndProc;
                    wc.lpszClassName = classNamePtr;
                    wc.hInstance = (HINSTANCE)hInstance;
                    wc.hbrBackground = new HBRUSH(PInvoke.GetStockObject(GET_STOCK_OBJECT_FLAGS.BLACK_BRUSH));
                    wc.style = WNDCLASS_STYLES.CS_VREDRAW | WNDCLASS_STYLES.CS_HREDRAW;
                    classId = PInvoke.RegisterClass(wc);

                    if (classId == 0)
                    {
                        throw new Exception("class not registered");
                    }
                }

                hwnd = PInvoke.CreateWindowEx(
                0,
                className,
                $"MinimalWebView {Assembly.GetExecutingAssembly().GetName().Version}",
                WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT,
                new HWND(),
                null,
                new CreateWindowHandle(hInstance, false),
                null);
            }

            if (hwnd.Value == 0)
                throw new Exception("hwnd not created");

            PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_NORMAL);

            CreateCoreWebView2(hwnd);

            MSG msg;
            while (PInvoke.GetMessage(out msg, new HWND(), 0, 0))
            {
                PInvoke.TranslateMessage(msg);
                PInvoke.DispatchMessage(msg);
            }
        }

        private static LRESULT WndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
        {
            // Initialize the WebView2 if the controller is ready. There's probably a more efficient place to do this.
            EnsureWebViewIsInitialized(hwnd);

            switch (msg)
            {
                case Constants.WM_SIZE:
                    int width = GetLowWord(lParam.Value);
                    int height = GetHighWord(lParam.Value);
                    OnSize(hwnd, wParam, width, height);

                    break;
                case Constants.WM_PAINT:
                    Paint(hwnd);
                    return (LRESULT)0;
                case Constants.WM_CLOSE: // unless we handle this manually the process will not quit when the window is closed
                    Environment.Exit(0);
                    break;
                default:
                    break;
            }

            return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
        }

        private static void RepaintWholeWindow(HWND hwnd)
        {
            RECT wholeWindow;
            PInvoke.GetClientRect(hwnd, out wholeWindow);
            PInvoke.InvalidateRect(hwnd, wholeWindow, false);
            PInvoke.UpdateWindow(hwnd);
        }

        private static void OnSize(HWND hwnd, WPARAM wParam, int width, int height)
        {
            if (_controller != null)
                _controller.Bounds = new Rectangle(0, 0, width, height);
        }

        private static void Paint(HWND hwnd)
        {
            HDC hdc;
            PAINTSTRUCT ps;

            hdc = PInvoke.BeginPaint(hwnd, out ps);
            // do painting here if needed
            PInvoke.EndPaint(hwnd, ps);
        }

        // Do post-creation initialization that needs to happen on the UI thread
        private static void EnsureWebViewIsInitialized(HWND hwnd)
        {
            if (_controller == null || _webView2HasBeenInitialized)
                return;

            RECT hwndRect;
            PInvoke.GetClientRect(hwnd, out hwndRect);

            _controller.CoreWebView2.SetVirtualHostNameToFolderMapping("minimalwebview.example", "wwwroot", CoreWebView2HostResourceAccessKind.Allow);
            _controller.Bounds = new Rectangle(0, 0, hwndRect.right, hwndRect.bottom);
            _controller.IsVisible = true;
            _controller.CoreWebView2.Navigate("https://minimalwebview.example/index.html");

            _webView2HasBeenInitialized = true;
        }

        private static void CreateCoreWebView2(HWND hwnd)
        {
            CoreWebView2Environment environment =
                CoreWebView2Environment.CreateAsync(null, null, null).Result;
            
            // would be nice to just wait on Result here, but that results in a deadlock
            environment.CreateCoreWebView2ControllerAsync(hwnd).ContinueWith(t =>
            {
               _controller = t.Result;
                RepaintWholeWindow(hwnd);
            });
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

        private class CreateWindowHandle : SafeHandle
        {
            public CreateWindowHandle(IntPtr invalidHandleValue, bool ownsHandle) : base(invalidHandleValue, ownsHandle)
            {
            }

            public override bool IsInvalid => false;
            protected override bool ReleaseHandle() => true;
        }
    }
}
