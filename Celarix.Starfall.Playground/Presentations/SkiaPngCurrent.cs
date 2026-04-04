using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Layout.Helium.Transitions;
using Celarix.Starfall.Presentation;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.Presentations
{
    internal static class SkiaPngCurrent
    {
        private static PresentationEngine<HeliumScene, IHeliumTransition>? presentationEngine;

        public static void Run()
        {
            var options = new SkiaPngTargetOptions
            {
                Width = 1280,
                Height = 720,
                FramesPerSecond = 60,
                OutputPath = @"E:\Documents\Files\Pictures\Miscellaneous\Starfall\0"
            };
            var layoutEngine = new HeliumLayoutEngine(options.Width, options.Height);
            presentationEngine = new PresentationEngine<HeliumScene, IHeliumTransition>(new PresentationEngineOptions
            {
                ErrorLevel = ErrorLevel.Display
            }, layoutEngine);
            var pngTarget = new SkiaPngTarget(options);
            var measuringService = new Rendering.MeasurementService(pngTarget);
        }
    }
}
