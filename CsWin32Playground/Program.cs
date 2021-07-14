using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks.Dataflow;
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
        private record WinMessage(uint msg, WPARAM wParam, LPARAM lParam);

        static private List<WinMessage> messages = new();

        static private Dictionary<uint, string> MessageNameLookup = WinMessageLookup.MessageNameDict();

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

            messages.Add(new WinMessage(msg, wParam, lParam));

            if (msg != Constants.WM_PAINT && messages.Count % 7 == 0)
            {
                RepaintWholeWindow(hwnd);
            }

            switch (msg)
            {
                //case Constants.WM_SIZE:
                //    {
                //        int width = GetLowWord(lParam.Value);   // Get the low-order word.
                //        int height = GetHighWord(lParam.Value); // Get the high-order word.

                //        return (LRESULT) 0;
                //    }
                case Constants.WM_MOUSEMOVE:

                    return (LRESULT) 0;
                case Constants.WM_PAINT:
                    Paint(hwnd, out hdc, out ps, out rect);
                    return (LRESULT) 0;
                default:
                    break;
            }

            //RepaintWholeWindow(hwnd);

            return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
        }

        private static string FormatMessagesExpensive(IEnumerable<WinMessage> messages)
        {
            StringBuilder ret = new();
            ret.AppendLine("Most Popular Window Messages:");
            ret.AppendLine();

            var grouped = messages.GroupBy((m) => m.msg)
                .Select(g => new { MsgName = GetPossibleMethodNames(g.Key), MsgCode = g.Key, Count = g.Count() })
                .OrderByDescending(m => m.Count)
                .Take(40);

            foreach (var item in grouped)
            {

                string cnt = $"{item.Count}x".PadRight(10, ' ');

                ret.Append(cnt);
                ret.Append(cnt.Length);
                ret.AppendLine($"{item.MsgName} (0x{item.MsgCode})");
            }

            return ret.ToString();

            static string GetPossibleMethodNames(uint msgCode)
            {
                return MessageNameLookup.ContainsKey(msgCode) ? MessageNameLookup[msgCode] : $"Unknown (0x{msgCode:x})";
            }
        }

        private static void RepaintWholeWindow(HWND hwnd)
        {
            RECT wholeWindow;
            PInvoke.GetClientRect(hwnd, out wholeWindow);

            // erasing the background is slow and can cause flicker. only do it infrequently
            bool eraseBackground = messages.Count % 2 == 0;
            PInvoke.InvalidateRect(hwnd, wholeWindow, true);
            PInvoke.UpdateWindow(hwnd);
        }

        private static void Paint(HWND hwnd, out HDC hdc, out PAINTSTRUCT ps, out RECT rect)
        {
            hdc = PInvoke.BeginPaint(hwnd, out ps);
            PInvoke.GetClientRect(hwnd, out rect);

            var hdcHandle = new HdcSafeHandle(hdc);

            string displayText = $"Message count: {messages.Count}" + Environment.NewLine + Environment.NewLine;
            
            displayText += FormatMessagesExpensive(messages);

            RECT offsetRect = rect;
            int offsetPx = 20;
            offsetRect.left += offsetPx;
            offsetRect.top += offsetPx;
            offsetRect.right -= offsetPx;
            offsetRect.bottom -= offsetPx;

            HGDIOBJ hFont, hOldFont;

            hFont =  PInvoke.GetStockObject(GET_STOCK_OBJECT_FLAGS.ANSI_FIXED_FONT);

            hOldFont = PInvoke.SelectObject(hdc, hFont);
            PInvoke.DrawText(hdcHandle, displayText, -1, ref offsetRect, DRAW_TEXT_FORMAT.DT_LEFT | DRAW_TEXT_FORMAT.DT_VCENTER);
            PInvoke.SelectObject(hdc, hOldFont);

            PInvoke.EndPaint(hwnd, ps);
        }

        private static void OnSize(HWND hwnd, WPARAM wParam, int width, int height)
        {

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
