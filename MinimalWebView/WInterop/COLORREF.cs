// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Drawing;
using System.Runtime.InteropServices;

namespace WInterop.Gdi.Native
{
    // https://msdn.microsoft.com/en-us/library/dd183449.aspx
    [StructLayout(LayoutKind.Explicit)]
    public struct COLORREF
    {
        [FieldOffset(0)]
        public byte R;
        [FieldOffset(1)]
        public byte G;
        [FieldOffset(2)]
        public byte B;

        [FieldOffset(0)]
        public uint Value;

        public COLORREF(byte red, byte green, byte blue)
        {
            Value = 0;
            R = red;
            G = green;
            B = blue;
        }

        public COLORREF(uint value)
        {
            R = 0;
            G = 0;
            B = 0;
            Value = value & 0x00FFFFFF;
        }

        public static COLORREF DIB_RGB_COLORS => 0;
        public static COLORREF DIB_PAL_COLORS => 1;

        public bool IsInvalid => Value == 0xFFFFFFFF;

        public override bool Equals(object? obj)
        {
            return obj is COLORREF other
                ? Equals(other)
                : false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public bool Equals(COLORREF other) => other.Value == Value;
        public static bool operator ==(COLORREF a, COLORREF b) => a.Value == b.Value;
        public static bool operator !=(COLORREF a, COLORREF b) => a.Value != b.Value;
        public static implicit operator COLORREF(uint value) => new COLORREF(value);
        public static implicit operator COLORREF(Color color) => new COLORREF(color.R, color.G, color.B);
        public static implicit operator Color(COLORREF color) => Color.FromArgb(color.R, color.G, color.B);
    }
}