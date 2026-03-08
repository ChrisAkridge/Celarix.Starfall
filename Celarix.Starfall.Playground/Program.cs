// See https://aka.ms/new-console-template for more information
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Elements.Containers;
using Celarix.Starfall.Layout.Helium.Transitions;
using Celarix.Starfall.Playground.FloatingPoint;
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
        private static int slideNumber;
        private static int slideCount;
        private static PresentationEngine<HeliumScene, IHeliumTransition>? presentationEngine;

        private static void Main(string[] args)
        {
            var engineOptions = new PresentationEngineOptions
            {
                ErrorLevel = ErrorLevel.Display
            };
            var layoutEngine = new HeliumLayoutEngine(1280, 720);
            presentationEngine = new PresentationEngine<HeliumScene, IHeliumTransition>(engineOptions, layoutEngine);
            var tkTarget = new SkiaTkTarget(1280, 720, 60, "Starfall Playground", presentationEngine);
            tkTarget.KeyUp += TkTarget_KeyUp;

            layoutEngine.SetRenderTarget(tkTarget);

            var singleConstruction = new SingleConstructionView((float)Math.PI);
            var scenes = new List<HeliumScene>();
            while (singleConstruction.MoveNext())
            {
                var element = singleConstruction.MakeWindowElement();
                var scene = CreateFPScene(element);
                scenes.Add(scene);
            }

            for (var i = 0; i < scenes.Count - 1; i++)
            {
                presentationEngine.AddScene($"scene{i}", scenes[i]);
            }
            slideCount = scenes.Count;

            presentationEngine.SetCurrentScene("scene0");
            presentationEngine.Start();
        }

        private static void TkTarget_KeyUp(object? sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == Keys.Right)
            {
                slideNumber += 1;
            }
            else if (e.Key == Keys.Left)
            {
                slideNumber -= 1;
            }

            slideNumber = Math.Clamp(slideNumber, 0, slideCount - 1);
            var sceneId = $"scene{slideNumber}";
            if (presentationEngine!.CurrentSceneId != sceneId)
            {
                presentationEngine.SetCurrentScene(sceneId);
            }
        }

        private static HeliumScene CreateFPScene(FloatingPointWindowElement element)
        {
            var container0 = new BinaryElementContainer();
            container0.SplitHorizontal(1, 4);
            var container1 = container0.SecondSplit!;
            container1.SplitHorizontal(1, 6);
            var container2 = container1.FirstSplit!;
            container2.SetSingleChild(element);

            var scene = new HeliumScene
            {
                BackgroundColor = new SColor(8, 0, 130, 255),
                Root = container0
            };

            return scene;
        }
    }
}