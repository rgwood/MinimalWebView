using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

[assembly:System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]

namespace CsWin32Playground
{
    // https://docs.microsoft.com/en-us/windows/win32/learnwin32/creating-a-window
    // https://docs.microsoft.com/en-us/windows/win32/learnwin32/window-messages
    // https://www.gitmemory.com/issue/microsoft/CsWin32/244/822066737
    class Program
    {
        static unsafe void Main(string[] args)
        {
            // Microsoft.Windows.Sdk.PInvoke
            // Microsoft.Windows.Sdk.Constants

            string className = "Sample Window Class";
            IntPtr hInstance = Process.GetCurrentProcess().Handle;

            ushort classId;

            fixed (char* classNamePtr = className)
            {
                WNDCLASSW wc = new WNDCLASSW();
                wc.lpfnWndProc = WndProc;
                wc.lpszClassName = classNamePtr;
                wc.hInstance = (HINSTANCE)hInstance;
                wc.hbrBackground = new HBRUSH(PInvoke.GetStockObject(GET_STOCK_OBJECT_FLAGS.LTGRAY_BRUSH));
                wc.style = WNDCLASS_STYLES.CS_VREDRAW | WNDCLASS_STYLES.CS_HREDRAW; 
                classId = PInvoke.RegisterClass(wc);

                if (classId == 0)
                {
                    throw new Exception("class not registered");
                }
            }
            
            HWND hwnd = PInvoke.CreateWindowEx(
                0,
                className,
                "Learn to Program Windows",
                WINDOW_STYLE.WS_OVERLAPPEDWINDOW,
                Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT, Constants.CW_USEDEFAULT,
                new HWND(),
                null,
                new CreateWindowHandle(hInstance, false),
                null);

            if (hwnd.Value == 0)
            {
                throw new Exception("hwnd not created");
            }

            PInvoke.ShowWindow(hwnd, SHOW_WINDOW_CMD.SW_NORMAL);
            
            MSG msg;
            while (PInvoke.GetMessage(out msg, new HWND(), 0, 0))
            {
                PInvoke.TranslateMessage(msg);
                PInvoke.DispatchMessage(msg);
            }
        }

        private static LRESULT WndProc(HWND hwnd, uint msg, WPARAM wParam, LPARAM lParam)
        {
            HDC hdc;
            PAINTSTRUCT ps;
            RECT rect;

            switch (msg)
            {
                case Constants.WM_CREATE:
                    break;
                case Constants.WM_SIZE:
                    {
                        int width = GetLowWord(lParam.Value);   // Get the low-order word.
                        int height = GetHighWord(lParam.Value); // Get the high-order word.

                        // Respond to the message:
                        OnSize(hwnd, wParam, width, height);
                    }
                    break;
                case Constants.WM_MOUSEMOVE:
                    Console.WriteLine(nameof(Constants.WM_MOUSEMOVE));
                    break;
                case Constants.WM_PAINT:
                    hdc = PInvoke.BeginPaint(hwnd, out ps);
                    PInvoke.GetClientRect(hwnd, out rect);

                    var hdcHandle = new HdcSafeHandle(hdc);

                    PInvoke.DrawText(hdcHandle, "Hello World", -1, ref rect, DRAW_TEXT_FORMAT.DT_SINGLELINE |  DRAW_TEXT_FORMAT.DT_CENTER | DRAW_TEXT_FORMAT.DT_VCENTER);
                    PInvoke.EndPaint(hwnd, ps);
                    break;
                default:
                    break;
            }

            return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
        }

        private static void OnSize(HWND hwnd, WPARAM wParam, int width, int height)
        {

        }

        private static int GetLowWord(nint value)
        {
            uint xy = (uint)value;
            int x = unchecked((short)xy);
            int y = unchecked((short)(xy >> 16));

            return x;
        }

        private static int GetHighWord(nint value)
        {
            uint xy = (uint)value;
            int x = unchecked((short)xy);
            int y = unchecked((short)(xy >> 16));

            return y;
        }

        public class CreateWindowHandle : SafeHandle
        {
            public CreateWindowHandle(IntPtr invalidHandleValue, bool ownsHandle) : 
                base(invalidHandleValue, ownsHandle)
            {
            }

            public override bool IsInvalid => false;

            protected override bool ReleaseHandle()
            {
                return true;   
            }
        }

        // TODO: no idea whether this is right
        public class HdcSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public HdcSafeHandle(HDC hdc) :
                base(ownsHandle: false)
            {
                SetHandle(hdc);
            }

            //public override bool IsInvalid => false;

            protected override bool ReleaseHandle()
            {
                // not clear to me whether this is needed.
                this.SetHandleAsInvalid();
                return true;
            }
        }
    }
}
