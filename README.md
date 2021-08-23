# MinimalWebView

## tl;dr

A tiny .NET 6 Windows application that hosts web UI in [WebView2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/). No UI framework, just a plain old Win32 message loop.

![screenshot](https://res.cloudinary.com/reilly-wood/image/upload/v1627014945/github-readmes/MinimalWebView.png)

This is very bare-bones and is mostly useful as an educational example. If you're looking for more functionality, my poorly-named [MaximalWebView](https://github.com/rgwood/MaximalWebView) builds on top of this.

## Motivation

This is an experiment to see how far I can push a slim Windows application that uses C# for the hosting logic and web UI for the "front-end".

The native UI situation on Windows is a little bleak and WebView2 will be shipping with Windows 11, so it's a good time to lean into web UI a little more. But... the .NET team has been killing it in recent years, I'd like to write my code that lives outside Chromium in C# or F#.

Microsoft provides WinForms and WPF wrappers for WebView2, so I could embed web UI in a plain old .NET GUI app. But WinForms and WPF are *big* dependencies and they're mostly unnecessary for web UI. Why not see how far we can get with an old-school Win32 message pump?

## Dependencies

[Microsoft.Web.WebView2.Core](https://www.nuget.org/packages/Microsoft.Web.WebView2/) to interact with WebView2, and [CsWin32](https://github.com/microsoft/CsWin32) to generate bindings to native Windows APIs (at compile time only).

## Known Issues

The resulting application cannot currently be [trimmed](https://docs.microsoft.com/en-us/dotnet/core/deploying/trimming-options) because of the way Microsoft.Web.WebView2.Core does COM interop. Please upvote [this issue](https://github.com/MicrosoftEdge/WebView2Feedback/issues/1490).

## Credits

Initially derived from Vítězslav Imrýšek's handy [CsWin32Playground]( https://github.com/VitezslavImrysek/CsWin32Playground), where he shows how to create a message pump using CsWin32.
