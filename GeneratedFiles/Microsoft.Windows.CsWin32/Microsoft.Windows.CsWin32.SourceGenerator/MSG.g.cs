﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------

#pragma warning disable CS1591,CS1573,CS0465,CS0649,CS8019,CS1570,CS1584,CS1658
namespace Windows.Win32
{
	using global::System;
	using global::System.Diagnostics;
	using global::System.Runtime.CompilerServices;
	using global::System.Runtime.InteropServices;
	using global::System.Runtime.Versioning;
	using win32 = global::Windows.Win32;

	namespace UI.WindowsAndMessaging
	{
			/// <summary>Contains message information from a thread's message queue.</summary>
			/// <remarks>
			/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msg">Learn more about this API from docs.microsoft.com.</see></para>
			/// </remarks>
		internal partial struct MSG
		{
			/// <summary>
			 /// <para>Type: <b>HWND</b> A handle to the window whose window procedure receives the message. This member is <b>NULL</b> when the message is a thread message.</para>
			/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msg#members">Read more on docs.microsoft.com</see>.</para>
			/// </summary>
			internal win32.Foundation.HWND hwnd;
			/// <summary>
			 /// <para>Type: <b>UINT</b> The message identifier. Applications can only use the low word; the high word is reserved by the system.</para>
			/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msg#members">Read more on docs.microsoft.com</see>.</para>
			/// </summary>
			internal uint message;
			/// <summary>
			 /// <para>Type: <b>WPARAM</b> Additional information about the message. The exact meaning depends on the value of the <b>message</b> member.</para>
			/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msg#members">Read more on docs.microsoft.com</see>.</para>
			/// </summary>
			internal win32.Foundation.WPARAM wParam;
			/// <summary>
			 /// <para>Type: <b>LPARAM</b> Additional information about the message. The exact meaning depends on the value of the <b>message</b> member.</para>
			/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msg#members">Read more on docs.microsoft.com</see>.</para>
			/// </summary>
			internal win32.Foundation.LPARAM lParam;
			/// <summary>
			 /// <para>Type: <b>DWORD</b> The time at which the message was posted.</para>
			/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msg#members">Read more on docs.microsoft.com</see>.</para>
			/// </summary>
			internal uint time;
			/// <summary>
			 /// <para>Type: <b><a href="https://docs.microsoft.com/previous-versions/dd162805(v=vs.85)">POINT</a></b> The cursor position, in screen coordinates, when the message was posted.</para>
			/// <para><see href="https://docs.microsoft.com/windows/win32/api//winuser/ns-winuser-msg#members">Read more on docs.microsoft.com</see>.</para>
			/// </summary>
			internal win32.Foundation.POINT pt;
		}
	}
}
