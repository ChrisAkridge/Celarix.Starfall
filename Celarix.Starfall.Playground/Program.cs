// See https://aka.ms/new-console-template for more information
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Layout.Helium.Components;
using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Elements.Containers;
using Celarix.Starfall.Layout.Helium.Transitions;
using Celarix.Starfall.Playground.FloatingPoint;
using Celarix.Starfall.Playground.Presentations;
using Celarix.Starfall.Playground.Starsong;
using Celarix.Starfall.Presentation;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using OpenTK.Graphics.ES11;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;

namespace Celarix.Starfall.Playground
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Please provide a presentation name as an argument.");
                return;
            }

            var presentationName = args[0].ToLowerInvariant();

            if (presentationName == "skiatk")
            {
                SkiaTkCurrent.Run();
            }
            else
            {
                Console.WriteLine("Unknown presentation name.");
            }
        }
    }
}