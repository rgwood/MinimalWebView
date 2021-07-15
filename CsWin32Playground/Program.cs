using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace CsWin32Playground
{
    class Program
    {
        private record RawWindowMessage(uint msg, WPARAM wParam, LPARAM lParam);

        static private List<RawWindowMessage> messages = new();

        static private Dictionary<uint, string> MessageCodesToNames = WinMsgUtils.MessageNameDict();

        static unsafe void Main(string[] args)
        {
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
                "Spy--",
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
            messages.Add(new RawWindowMessage(msg, wParam, lParam));

            switch (msg)
            {
                case Constants.WM_KEYDOWN:
                    if ((nuint)wParam == Constants.VK_SPACE)
                    {
                        // SLOW! Simulate slow IO
                        Thread.Sleep(5000);
                        messages.Clear();
                        RepaintWholeWindow(hwnd);
                    }
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

            //repaint everything on an arbitrary interval
            if (msg != Constants.WM_PAINT && messages.Count % 7 == 0)
            {
                RepaintWholeWindow(hwnd);
            }

            return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
        }

        private static string FormatMessagesExpensive(IEnumerable<RawWindowMessage> messages)
        {
            StringBuilder ret = new();
            ret.AppendLine("Most Popular Window Messages (press space to reset):");
            ret.AppendLine();

            var grouped = messages.GroupBy((m) => m.msg)
                .Select(g => new { MsgName = GetPossibleMethodNames(g.Key), MsgCode = g.Key, Count = g.Count() })
                .OrderByDescending(m => m.Count)
                .Take(40);

            foreach (var item in grouped)
            {
                ret.Append($"{item.Count}x".PadRight(10));
                ret.AppendLine($"{item.MsgName} (0x{item.MsgCode})");
            }

            return ret.ToString();

            static string GetPossibleMethodNames(uint msgCode)
            {
                return MessageCodesToNames.ContainsKey(msgCode) ? MessageCodesToNames[msgCode] : $"Unknown (0x{msgCode:x})";
            }
        }

        private static void RepaintWholeWindow(HWND hwnd)
        {
            RECT wholeWindow;
            PInvoke.GetClientRect(hwnd, out wholeWindow);

            // TODO: erasing the background every time is flickery. can we work out when it's necessary?
            PInvoke.InvalidateRect(hwnd, wholeWindow, true);
            PInvoke.UpdateWindow(hwnd);
        }

        private static void Paint(HWND hwnd)
        {
            HDC hdc;
            PAINTSTRUCT ps;
            RECT rect;

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
            protected override bool ReleaseHandle() => true;
        }

        // TODO: no idea whether this is right
        public class HdcSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public HdcSafeHandle(HDC hdc) : base(ownsHandle: false)
            {
                SetHandle(hdc);
            }

            // don't actually need to release anything, EndPaint is responsible for that
            protected override bool ReleaseHandle() => true;
        }
    }
}
