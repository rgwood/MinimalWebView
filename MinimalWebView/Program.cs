using Microsoft.Web.WebView2.Core;
using System;
using System.Drawing;
using System.IO;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RxFileSystemWatcher;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using System.Diagnostics;
using System.Linq;
using CliWrap;

namespace MinimalWebView
{
    class Program
    {
        internal const uint WM_SYNCHRONIZATIONCONTEXT_WORK_AVAILABLE = Constants.WM_USER + 1;
        private const string StaticFileDirectory = "wwwroot";
        private static CoreWebView2Controller _controller;
        private static SingleThreadSynchronizationContext _uiThreadSyncCtx;
        
        // hot reload stuff
        private const string NpxPath = @"C:\Program Files\nodejs\npx.cmd";
        private static ObservableFileSystemWatcher _wwwRootFileSystemWatcher;
        private static CommandTask<CommandResult> _npxTask;
        private static CancellationTokenSource _npxTaskCTS;

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
                        throw new Exception("class not registered");
                }

                fixed (char* windowNamePtr = $"MinimalWebView {Assembly.GetExecutingAssembly().GetName().Version}")
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

            if (hwnd.Value == 0)
                throw new Exception("hwnd not created");

            PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_NORMAL);

            _uiThreadSyncCtx = new SingleThreadSynchronizationContext(hwnd);
            SynchronizationContext.SetSynchronizationContext(_uiThreadSyncCtx);

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
                case WM_SYNCHRONIZATIONCONTEXT_WORK_AVAILABLE:
                    _uiThreadSyncCtx.RunAvailableWorkOnCurrentThread();
                    break;
                case Constants.WM_CLOSE:
                    _npxTaskCTS?.Cancel();
                    PInvoke.PostQuitMessage(0);
                    break;
            }

            return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
        }

        private static void OnSize(HWND hwnd, WPARAM wParam, int width, int height)
        {
            if (_controller != null)
                _controller.Bounds = new Rectangle(0, 0, width, height);
        }

        private static async void CreateCoreWebView2(HWND hwnd)
        {
            var environment = await CoreWebView2Environment.CreateAsync(null, null, null);

            _controller = await environment.CreateCoreWebView2ControllerAsync(hwnd);

            PInvoke.GetClientRect(hwnd, out var hwndRect);

            _controller.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            _controller.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoadedFirstTime;

            _controller.CoreWebView2.SetVirtualHostNameToFolderMapping("minimalwebview.example", StaticFileDirectoryPath, CoreWebView2HostResourceAccessKind.Allow);
            _controller.Bounds = new Rectangle(0, 0, hwndRect.right, hwndRect.bottom);
            _controller.IsVisible = true;
            _controller.CoreWebView2.Navigate("https://minimalwebview.example/index.html");
        }

        // Set up Hot Reload once at startup
        private static async void CoreWebView2_DOMContentLoadedFirstTime(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            _controller.CoreWebView2.DOMContentLoaded -= CoreWebView2_DOMContentLoadedFirstTime;

            if (Debugger.IsAttached)
            {
                try
                {
                    SetupAndStartFileSystemWatcher();
                    await SetupAndRunTailwindJIT();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error in Hot Reload:");
                    Debug.WriteLine(ex.Demystify().ToString());
                }
            }
        }

        // If a debugger is attached (i.e. we're doing hot reload), get the path relative to the project directory
        static string StaticFileDirectoryPath
            => Debugger.IsAttached ? Path.Combine(ProjectDirectoryPath.Value, StaticFileDirectory) : StaticFileDirectory;

        private static async Task SetupAndRunTailwindJIT()
        {
            // TODO: clean up any orphaned Node processes from previous runs
            // We handle cleanup when the application is closed gracefully, but closing the VS debugger
            // terminates the application with no opportunity for cleanup
            _npxTaskCTS = new CancellationTokenSource();

            string NodePath = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), @"nodejs\node.exe");
            string TailwindCliPath = Path.Combine(Environment.GetEnvironmentVariable("AppData"), @"npm\node_modules\tailwindcss\lib\cli.js");

            // We are calling node.exe directly instead of npx because npx introduces a
            // ghost process that hangs around even if the task is cancelled :(
            await Cli.Wrap(NodePath)
                .WithArguments(new string[] {
                    TailwindCliPath,
                    "-i",
                    "tailwind-input.css",
                    "-o",
                    "tailwind.css",
                    "--watch",
                    "--jit",
                    "--purge=./*.html" })
                .WithWorkingDirectory(StaticFileDirectoryPath)
                .WithStandardOutputPipe(PipeTarget.ToDelegate(l => Debug.WriteLine(l)))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(l => Debug.WriteLine(l)))
                .ExecuteAsync(_npxTaskCTS.Token);
        }

        private static void SetupAndStartFileSystemWatcher()
        {
            _wwwRootFileSystemWatcher = new ObservableFileSystemWatcher(new FileSystemWatcher(StaticFileDirectoryPath));
            _wwwRootFileSystemWatcher.Start();

            _wwwRootFileSystemWatcher.Changed
                .Concat(_wwwRootFileSystemWatcher.Created)
                .Concat(_wwwRootFileSystemWatcher.Deleted)
                .Concat(_wwwRootFileSystemWatcher.Renamed)
                .Buffer(TimeSpan.FromMilliseconds(20))
                .Where(x => x.Any())
                .ObserveOn(_uiThreadSyncCtx)
                .Subscribe(args =>
                {
                    Debug.WriteLine($"FileSystemEvent: {string.Join(',', args.Select(a => $"{a.ChangeType} {a.Name}"))}");
                    _controller.CoreWebView2.Reload();
                });
        }

        private static async void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var webMessage = e.TryGetWebMessageAsString();
            if (string.IsNullOrEmpty(webMessage))
                return;

            // simulate moving some slow operation to a background thread
            await Task.Run(() => Thread.Sleep(200));

            // this will blow up if not run on the UI thread, so the SynchronizationContext needs to have been wired up correctly
            await _controller.CoreWebView2.ExecuteScriptAsync($"alert('Hi from the UI thread! I got a message from the browser: {webMessage}')");
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
