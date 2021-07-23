# MinimalWebView

## tl;dr

A tiny .NET 6 Windows application that hosts web UI in [WebView2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/). No UI frameworks, just a plain old Win32 message loop.

![screenshot](https://res.cloudinary.com/reilly-wood/image/upload/v1627014945/github-readmes/MinimalWebView.png)

## Motivation

This is an experiment to see how far I can push a slim, minimal Windows application that uses C# for the hosting logic and web UI for the "front-end".

The native UI situation on Windows is a little bleak and WebView2 will be shipping with Windows 11, so it's a good time to lean into web UI a little more. But... the .NET team has been killing it in recent years, I'd like to write my code that lives outside Chromium in C# or F#.

Microsoft provides WinForms and WPF wrappers for WebView2, so I could just embed web UI in a plain old .NET GUI app. But WinForms and WPF are *big* dependencies and they're mostly unnecessary if web UI is my primary focus. Why not see how far we can get with an old-school Win32 message pump?

## Dependencies

[CsWin32](https://github.com/microsoft/CsWin32) for generating bindings to native Windows APIs, and [Microsoft.Web.WebView2.Core](https://www.nuget.org/packages/Microsoft.Web.WebView2/) which handles COM interop with WebView2. That's it!

## Disclaimer + Known Issues

I hacked this together in a few evenings. This is probably broken in many subtle ways and you should not use it in production. But it works for me :)

Trimmed self-contained executables seem to fail - possibly related to [this issue](https://github.com/MicrosoftEdge/WebView2Feedback/issues/1490). 

## Credits

Initially derived from Vítězslav Imrýšek's handy [CsWin32Playground]( https://github.com/VitezslavImrysek/CsWin32Playground) repo, where he shows how to create an old-school message pump using CsWin32.