// Floating Point Numbers, Visualized
// for the Kentucky Open Source Society
// July 2026


// Console Controller
//	Modes:
//		-Floating Point Path: Dark blue background
//        - Starfall Path: Dark teal background
//	Commands:
//		- Right arrow (or whatever the Logitech presentation clicker uses): Advance to the next slide/advance within current slide
//		- Left arrow: Go back to the previous slide/go back within current slide

using Celarix.Starfall;
using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Presentation;
using Celarix.Starfall.Presentations;
using Celarix.Starfall.Presentations.FloatingPoint;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Targets;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Image = SixLabors.ImageSharp.Image;

internal class Program
{
    // Working out command line arguments:
    // Celarix.Starfall.Presentations.exe <viewportWidth> <viewportHeight>

    [STAThread]
    private static void Main(string[] args)
    {
        if (args.Length is not (2 or 3))
        {
            Usage();
            return;
        }

        if (!int.TryParse(args[0], out var viewportWidth))
        {
            Usage();
            return;
        }

        if (!int.TryParse(args[1], out var viewportHeight))
        {
            Usage();
            return;
        }

        var reinitializeOnLastChanceException = false;
        if (args.Length == 3 && args[2].Equals("--reinit", StringComparison.OrdinalIgnoreCase))
        {
            reinitializeOnLastChanceException = true;
        }

        var initArgs = new PresentationInitializationArguments
        {
            ViewportWidth = viewportWidth,
            ViewportHeight = viewportHeight,
            ReinitializeOnLastChanceException = reinitializeOnLastChanceException
        };

        var runPresentation = true;
        while (runPresentation)
        {
            var runner = new PresentationRunner(initArgs);

            try
            {
                runner.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught last-chance exception in Main: " + ex);

                // Last best hope handling here, just remake the entire darn thing and try again
                if (!reinitializeOnLastChanceException)
                {
                    runPresentation = false;
                }
            }
        }
    }

    private static void Usage()
    {
        Console.WriteLine("Usage: Celarix.Starfall.Presentations.exe <viewportWidth> <viewportHeight>");
        Environment.Exit(1);
    }
}