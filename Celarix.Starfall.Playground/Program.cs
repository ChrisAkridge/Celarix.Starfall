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

            var slide = new HeliumScene();
            var binaryElementContainer = new BinaryElementContainer
            {
                Alignment = Alignment.Center
            };
            binaryElementContainer.SplitVertical(1, 1);

            var blueRect = new RectangleElement(0.5d, 0.5d, new SColor(0, 0, 255, 255), "blue-rect");
            var greenRect = new RectangleElement(0.5d, 0.5d, new SColor(0, 255, 0, 255), "green-rect");
            var redRect = new RectangleElement(0.5d, 0.5d, new SColor(255, 0, 0, 255), "red-rect");
            binaryElementContainer.FirstSplit!.SplitHorizontal(1, 1);
            binaryElementContainer.SecondSplit!.SetSingleChild(greenRect);
            binaryElementContainer.FirstSplit.FirstSplit!.SetSingleChild(blueRect);
            binaryElementContainer.FirstSplit.SecondSplit!.SetSingleChild(redRect);
            slide.Root = binaryElementContainer;

            presentationEngine.AddScene(nameof(slide), slide);
            presentationEngine.SetCurrentScene(nameof(slide));

            presentationEngine.Start();
        }
    }
}