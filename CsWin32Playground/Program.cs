using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Windows.Sdk;

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
    }
}
