using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Playground.AtriaTests;
using Celarix.Starfall.Playground.AtriaTests.CanonicalDecomposition;
using Celarix.Starfall.Playground.AtriaTests.Operations;
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

            // var timeProgressSlide = new TimeProgressSlide(1280, 720);
            // var timeProgressSlide = new GigasecondSlide(1920, 188);
            // var timeProgressSlide = new ByteOperationSlide((x, y) => (byte)(x & y), "x & y", 1280, 720);
            // var timeProgressSlide = new ShortOperationSlide((x, y) => Quadrant(x), "quadrant(x)", 1280, 720);
            var timeProgressSlide = new CanonicalDecompositionSlide(@"E:\Documents\Files\Pictures\Pictures\S Series\1s Series\1s000335.png",
                1280, 720);
            layoutEngine.AddSlide(timeProgressSlide, "timeProgress");
            layoutEngine.SetCurrentSlide("timeProgress");
            layoutEngine.Start();
        }

        private static short Quadrant(short value)
        {
            var unsigned = (ushort)value;
            var y = (byte)(unsigned >> 8);
            var x = (byte)(unsigned & 0xFF);
            ushort interleaved = 0;
            interleaved |= (ushort)((x >> 7) << 15);
            interleaved |= (ushort)((y >> 7) << 14);
            interleaved |= (ushort)((x >> 6) << 13);
            interleaved |= (ushort)((y >> 6) << 12);
            interleaved |= (ushort)((x >> 5) << 11);
            interleaved |= (ushort)((y >> 5) << 10);
            interleaved |= (ushort)((x >> 4) << 9);
            interleaved |= (ushort)((y >> 4) << 8);
            interleaved |= (ushort)((x >> 3) << 7);
            interleaved |= (ushort)((y >> 3) << 6);
            interleaved |= (ushort)((x >> 2) << 5);
            interleaved |= (ushort)((y >> 2) << 4);
            interleaved |= (ushort)((x >> 1) << 3);
            interleaved |= (ushort)((y >> 1) << 2);
            interleaved |= (ushort)((x >> 0) << 1);
            interleaved |= (ushort)(y >> 0);
            return (short)interleaved;
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
