using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Playground.AtriaTests;
using Celarix.Starfall.Presentation;
using Celarix.Starfall.Rendering.Targets;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.Presentations
{
    internal static class AtriaCurrent
    {
        public static void Run()
        {
            var engineOptions = new PresentationEngineOptions
            {
                ErrorLevel = ErrorLevel.Display
            };
            var layoutEngine = new AtriaLayoutEngine(1280, 720);
            var tkTarget = new SkiaTkTarget(1280, 720, 60, "Starfall Playground", layoutEngine);
            tkTarget.KeyUp += TkTarget_KeyUp;

            layoutEngine.SetRenderTarget(tkTarget);
            var measurementService = new Rendering.MeasurementService(tkTarget);
            layoutEngine.MeasurementService = measurementService;

            var timeProgressSlide = new TimeProgressSlide(1280, 720);
            layoutEngine.AddSlide(timeProgressSlide, "timeProgress");
            layoutEngine.SetCurrentSlide("timeProgress");
            layoutEngine.Start();
        }

        private static void TkTarget_KeyUp(object? sender, KeyboardKeyEventArgs e)
        {
            //if (e.Key == Keys.Right)
            //{
            //    slideNumber += 1;
            //}
            //else if (e.Key == Keys.Left)
            //{
            //    slideNumber -= 1;
            //}

            //slideNumber = Math.Clamp(slideNumber, 0, slideCount - 1);
            //var sceneId = $"scene{slideNumber}";
            //if (presentationEngine!.CurrentSceneId != sceneId)
            //{
            //    presentationEngine.SetCurrentScene(sceneId);
            //}
        }
    }
}
