// See https://aka.ms/new-console-template for more information
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Layout.Helium.Components;
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
            
            do
            {
                var scene = CreateFPScene(singleConstruction);
                scenes.Add(scene);
            } while (singleConstruction.MoveNext());

            // brain kinda mush, very hackish
            scenes.Add(CreateFPScene(singleConstruction)); // add the final scene again so that the "next" button works on the last slide

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

        private static HeliumScene CreateFPScene(SingleConstructionView singleConstruction)
        {
            var element = singleConstruction.MakeWindowElement();
            var stack = new StackContainer(Direction.Vertical)
            {
                Alignment = Alignment.LeftCenter,
                Padding = new Padding(0.1d, 0.1d, 0.1d, 0.1d)
            };
            stack.AddChild(element, 2);

            var textLines = singleConstruction.GetDisplayText()
                .Select(l =>
                {
                    var textElement = new TextElement
                    {
                        Text = l,
                        Color = SColor.White,
                        Font = new SFontFamily("Consolas", 20f),
                        Alignment = Alignment.LeftCenter
                    };
                    //textElement.SetDesiredHeightFraction(0.3f);
                    //textElement.SetDesiredWidthFraction(0.3f);
                    return textElement;
                });

            foreach (var textLine in textLines)
            {
                stack.AddChild(textLine, 1);
            }

            // Add a few empty spacers at the end to push everything up a bit
            for (var i = 0; i < 3; i++)
            {
                stack.AddChild(null, 1);
            }

            var scene = new HeliumScene
            {
                BackgroundColor = new SColor(8, 0, 130, 255),
                Root = stack
            };

            return scene;
        }
    }
}