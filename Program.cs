namespace MinimalWebView;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.Drawing;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

class Program
{
    internal const uint WM_SYNCHRONIZATIONCONTEXT_WORK_AVAILABLE = Constants.WM_USER + 1;
    private const string StaticFileDirectory = "wwwroot";
    private static CoreWebView2Controller _controller;
    private static UiThreadSynchronizationContext _uiThreadSyncCtx;
    private static Stopwatch _timeSinceLaunch = Stopwatch.StartNew();

    [STAThread]
    static int Main(string[] args)
    {
        HWND hwnd;

        unsafe
        {
            HINSTANCE hInstance = PInvoke.GetModuleHandle((char*)null);
            ushort classId;

            fixed (char* classNamePtr = "MinimalWebView")
            {
                WNDCLASSW wc = new WNDCLASSW();
                wc.lpfnWndProc = WndProc;
                wc.lpszClassName = classNamePtr;
                wc.hInstance = hInstance;
                wc.hbrBackground = (HBRUSH)(IntPtr)PInvoke.GetStockObject(GET_STOCK_OBJECT_FLAGS.WHITE_BRUSH);
                wc.style = WNDCLASS_STYLES.CS_VREDRAW | WNDCLASS_STYLES.CS_HREDRAW;
                classId = PInvoke.RegisterClass(wc);

                if (classId == 0)
                    throw new Exception("class not registered");
            }

            fixed (char* windowNamePtr = "MinimalWebView")
            {
                hwnd = PInvoke.CreateWindowEx(
                    0,
                    (char*)classId,
                    windowNamePtr,
                    WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                    Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT, 600, 500,
                    new HWND(),
                    new HMENU(),
                    hInstance,
                    null);
            }
        }

        PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_NORMAL);

        _uiThreadSyncCtx = new UiThreadSynchronizationContext(hwnd);
        SynchronizationContext.SetSynchronizationContext(_uiThreadSyncCtx);

        InitializeCoreWebView2(hwnd);

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
            case WM_SYNCHRONIZATIONCONTEXT_WORK_AVAILABLE:
                _uiThreadSyncCtx.RunAvailableWorkOnCurrentThread();
                break;
            case Constants.WM_CLOSE:
                PInvoke.PostQuitMessage(0);
                break;
        }

        return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
    }

    private static async void InitializeCoreWebView2(HWND hwnd)
    {
        Log("Initializing WebView2...");
        var environment = await CoreWebView2Environment.CreateAsync(null, null, null);
        _controller = await environment.CreateCoreWebView2ControllerAsync(hwnd);
        Log("WebView2 initialized.");

        _controller.CoreWebView2.DOMContentLoaded += (_, _) => Log("Done! DOM content loaded");

        _controller.CoreWebView2.SetVirtualHostNameToFolderMapping("minimalwebview.example", StaticFileDirectory, CoreWebView2HostResourceAccessKind.Allow);
        PInvoke.GetClientRect(hwnd, out var hwndRect);

        _controller.Bounds = new Rectangle(0, 0, hwndRect.right, hwndRect.bottom);
        _controller.IsVisible = true;

        Log("Navigating...");

        _controller.CoreWebView2.Navigate("https://minimalwebview.example/index.html");
    }

    private static void Log(string s) 
        => Console.WriteLine($"{_timeSinceLaunch.ElapsedMilliseconds}ms: {s}");
}
