// See https://aka.ms/new-console-template for more information
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Elements.Containers;
using Celarix.Starfall.Layout.Helium.Transitions;
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
            var engineOptions = new PresentationEngineOptions
            {
                ErrorLevel = ErrorLevel.Display
            };
            var layoutEngine = new HeliumLayoutEngine(1280, 720);
            var presentationEngine = new PresentationEngine<HeliumScene, IHeliumTransition>(engineOptions, layoutEngine);
            var tkTarget = new SkiaTkTarget(1280, 720, 60, "Starfall Playground", presentationEngine);

            layoutEngine.SetRenderTarget(tkTarget);

            var firstSlide = new HeliumScene();
            var singleElementContainer = new SingleElementContainer()
            {
                Alignment = Alignment.Center,
                Child = new RectangleElement(0.3d, 0.3d, new SColor(0, 0, 255, 255), "blue-rect")
            };
            firstSlide.Root = singleElementContainer;
            presentationEngine.AddScene(nameof(firstSlide), firstSlide);

            var secondSlide = firstSlide.Clone();
            var secondSEC = secondSlide.Root as SingleElementContainer;
            secondSEC!.Alignment = Alignment.TopLeft;
            presentationEngine.AddScene(nameof(secondSlide), secondSlide);

            var transition = new FirstTransition(firstSlide, secondSlide, 0.5d, new SSizeF(1280, 720));
            presentationEngine.AddTransition(nameof(firstSlide), nameof(secondSlide), transition);

            presentationEngine.SetCurrentScene(nameof(firstSlide));

            // Register callback in 2 seconds to perform the transition
            var timer = new System.Timers.Timer(2000);
            timer.Start();
            timer.Elapsed += (sender, e) =>
            {
                // Stop the timer first! Otherwise it will keep firing every 2 seconds, which we don't want.
                timer.Stop();
                presentationEngine.SetCurrentScene(nameof(secondSlide));
            };

            presentationEngine.Start();
        }
    }
}