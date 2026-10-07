using Avalonia;
using ExtendedNumerics;

namespace Celarix.Starfall.Harness;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Same BigDecimal settings as the Playground; charts divide BigDecimals every frame.
        BigDecimal.Precision = 20;
        BigDecimal.AlwaysTruncate = true;
        BigDecimal.AlwaysNormalize = true;

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace();
}
