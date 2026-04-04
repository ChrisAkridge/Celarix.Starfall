using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Layout.Helium.Components;
using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Layout.Helium.Elements.Containers;
using Celarix.Starfall.Layout.Helium.Transitions;
using Celarix.Starfall.Playground.FloatingPoint;
using Celarix.Starfall.Playground.Starsong;
using Celarix.Starfall.Presentation;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.Presentations
{
    internal static class SkiaTkCurrent
    {
        private static int slideNumber;
        private static int slideCount;
        private static PresentationEngine<HeliumScene, IHeliumTransition>? presentationEngine;

        public static void Run()
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

            var measurementService = new Rendering.MeasurementService(tkTarget);
            var scenes = StarsongPresentationBuilder.BuildSlides();

            for (var i = 0; i < scenes.Count; i++)
            {
                presentationEngine.AddScene($"scene{i}", scenes[i]);
                if (i > 0)
                {
                    presentationEngine.AddTransition($"scene{i - 1}", $"scene{i}", StarsongPresentationBuilder.BuildSlideToSlideTransition(
                        scenes[i - 1], scenes[i], new SSizeF(1280, 720), measurementService));
                }
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
                Padding = new Padding(0.025d, 0d, 0d, 0d)
            };
            stack.AddChild(element, 2);

            var textLines = singleConstruction.GetDisplayText();
            var advancedTextElement = new AdvancedTextElement(string.Join("\n", textLines))
            {
                Font = new SFontFamily("Calibri", 24f),
                Color = SColor.White,
                Rotation = SAngle.Zero,
                Alignment = Alignment.LeftCenter,
                LineSpacingMultiplier = 0.2d,
                Id = "details"
            };

            stack.AddEmpty(1);
            stack.AddChild(advancedTextElement, 2);
            stack.AddEmpty(2);

            var scene = new HeliumScene
            {
                BackgroundColor = new SColor(8, 0, 130, 255),
                Root = stack
            };

            return scene;
        }
    }
}
