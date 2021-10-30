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


		/// <content>
		/// Contains extern methods from "Kernel32.dll".
		/// </content>
	internal static partial class PInvoke
	{
		/// <summary>Frees the loaded dynamic-link library (DLL) module and, if necessary, decrements its reference count.</summary>
		/// <param name="hLibModule">
		 /// <para>A handle to the loaded library module. The <a href="https://docs.microsoft.com/windows/desktop/api/libloaderapi/nf-libloaderapi-loadlibrarya">LoadLibrary</a>, <a href="https://docs.microsoft.com/windows/desktop/api/libloaderapi/nf-libloaderapi-loadlibraryexa">LoadLibraryEx</a>, <a href="https://docs.microsoft.com/windows/desktop/api/libloaderapi/nf-libloaderapi-getmodulehandlea">GetModuleHandle</a>, or <a href="https://docs.microsoft.com/windows/desktop/api/libloaderapi/nf-libloaderapi-getmodulehandleexa">GetModuleHandleEx</a> function returns this handle.</para>
		/// <para><see href="https://docs.microsoft.com/windows/win32/api//libloaderapi/nf-libloaderapi-freelibrary#parameters">Read more on docs.microsoft.com</see>.</para>
		/// </param>
		/// <returns>
		 /// <para>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero. To get extended error information, call the <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a> function.</para>
		/// </returns>
		/// <remarks>
		/// <para><see href="https://docs.microsoft.com/windows/win32/api//libloaderapi/nf-libloaderapi-freelibrary">Learn more about this API from docs.microsoft.com.</see></para>
		/// </remarks>
		[DllImport("Kernel32", ExactSpelling = true,SetLastError = true)]
[DefaultDllImportSearchPaths(DllImportSearchPath.System32)][SupportedOSPlatform("windows5.1.2600")]internal static extern win32.Foundation.BOOL FreeLibrary(win32.Foundation.HINSTANCE hLibModule);

		/// <inheritdoc cref= "GetModuleHandle(win32.Foundation.PCWSTR )"/>
		[SupportedOSPlatform("windows5.1.2600")]internal static unsafe FreeLibrarySafeHandle GetModuleHandle(string lpModuleName)
		{
			fixed (char* lpModuleNameLocal = lpModuleName)

				{
win32.Foundation.HINSTANCE __result = PInvoke.GetModuleHandle(lpModuleNameLocal);
					return new FreeLibrarySafeHandle(__result,ownsHandle: true);
				}
		}

		/// <summary>Retrieves a module handle for the specified module. The module must have been loaded by the calling process.</summary>
		/// <param name="lpModuleName">
		 /// <para>The name of the loaded module (either a .dll or .exe file). If the file name extension is omitted, the default library extension .dll is appended. The file name string can include a trailing point character (.) to indicate that the module name has no extension. The string does not have to specify a path. When specifying a path, be sure to use backslashes (\\), not forward slashes (/). The name is compared (case independently) to the names of modules currently mapped into the address space of the calling process.</para>
		/// <para>If this parameter is NULL, <b>GetModuleHandle</b> returns a handle to the file used to create the calling process (.exe file). The <b>GetModuleHandle</b> function does not retrieve handles for modules that were loaded using the <b>LOAD_LIBRARY_AS_DATAFILE</b> flag. For more information, see <a href="https://docs.microsoft.com/windows/desktop/api/libloaderapi/nf-libloaderapi-loadlibraryexa">LoadLibraryEx</a>.</para>
		/// <para><see href="https://docs.microsoft.com/windows/win32/api//libloaderapi/nf-libloaderapi-getmodulehandlew#parameters">Read more on docs.microsoft.com</see>.</para>
		/// </param>
		/// <returns>
		 /// <para>If the function succeeds, the return value is a handle to the specified module. If the function fails, the return value is NULL. To get extended error information, call <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>.</para>
		/// </returns>
		/// <remarks>
		/// <para><see href="https://docs.microsoft.com/windows/win32/api//libloaderapi/nf-libloaderapi-getmodulehandlew">Learn more about this API from docs.microsoft.com.</see></para>
		/// </remarks>
		[DllImport("Kernel32", ExactSpelling = true,EntryPoint = "GetModuleHandleW",SetLastError = true)]
[DefaultDllImportSearchPaths(DllImportSearchPath.System32)][SupportedOSPlatform("windows5.1.2600")]internal static extern win32.Foundation.HINSTANCE GetModuleHandle(win32.Foundation.PCWSTR lpModuleName);
	}
}