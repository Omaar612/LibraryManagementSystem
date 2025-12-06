open Avalonia
open Avalonia.ReactiveUI
open LibrarySystemAvalonia

[<EntryPoint>]
let main argv =
    AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace()
        .UseReactiveUI()
        .StartWithClassicDesktopLifetime(argv)
