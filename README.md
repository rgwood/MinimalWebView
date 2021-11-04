# WebView2 .NET Single-File Performance Repro

This branch demonstrates a curious performance issue with WebView2 and .NET 6 single-file executables.

The repo contains 2 publish profiles which can be used to publish self-contained applications; one [single-file](https://docs.microsoft.com/en-us/dotnet/core/deploying/single-file) and one multi-file.

```
dotnet publish --configuration Release -p:PublishProfile=SingleFile
dotnet publish --configuration Release -p:PublishProfile=MultiFile
```

Startup time (as measured by time until the first DOMContentLoaded event) is typically at least twice as slow for the single-file application. The difference is noticeable when launching each exe manually, and here are some crude but representative `Stopwatch` benchmarks from my machine:


Single-file:

```
2ms: Initializing WebView2...
342ms: CreateCoreWebView2ControllerAsync() returned
844ms: DOMContentLoaded event fired
```

Multi-file:

```
2ms: Initializing WebView2...
196ms: CreateCoreWebView2ControllerAsync() returned
320ms: DOMContentLoaded event fired
```