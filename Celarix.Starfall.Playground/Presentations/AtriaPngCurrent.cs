using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Playground.AtriaTests;
using Celarix.Starfall.Playground.AtriaTests.Operations;
using Celarix.Starfall.Presentation;
using Celarix.Starfall.Rendering.Targets;
using Celarix.Starfall.Rendering.Models;
using SixLabors.ImageSharp.Metadata;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SixLabors.ImageSharp;

namespace Celarix.Starfall.Playground.Presentations
{
    internal static class AtriaPngCurrent
    {
        public static void Run()
        {
            const string Image = @"E:\Documents\Files\Pictures\Pictures\S Series\1s Series\1s000001.png";
            var info = GetImageInfo(Image);
            var metadata = info.Metadata;

            var engineOptions = new PresentationEngineOptions
            {
                ErrorLevel = ErrorLevel.Display
            };
            var layoutEngine = new AtriaLayoutEngine(info.Width, info.Height);
            using SkiaPngTarget pngTarget = new SkiaPngTarget(new SkiaPngTargetOptions
            {
                Width = info.Width,
                Height = info.Height,
                FramesPerSecond = 60,
                OutputPath = Path.Combine(@"E:\Documents\Files\Pictures\Miscellaneous\Starfall\ImageTransforms", NextTransformIndex())
            });
            layoutEngine.SetRenderTarget(pngTarget);
            var measurementService = new Rendering.MeasurementService(pngTarget);
            layoutEngine.MeasurementService = measurementService;

            var imageTransformSlide = new ImageTransformSlide(Image,
                info.Width, info.Height,
                position =>
                {
                    uint pixelIndex = (uint)position.Y * (uint)info.Width + (uint)position.X;
                    var targetPixelIndex = (~pixelIndex) % ((uint)info.Width * (uint)info.Height);
                    return new SPointF(targetPixelIndex % (uint)info.Width, targetPixelIndex / (uint)info.Width);
                },
                color => color);
            layoutEngine.AddSlide(imageTransformSlide, "Image Transform");
            layoutEngine.SetCurrentSlide("Image Transform");

            var frameNumber = 0;
            while (!imageTransformSlide.Completed)
            {
                Console.WriteLine($"\tRendering frame {frameNumber++}");
                layoutEngine.OnFrameRequested(1.0 / 60);
            }
        }

        private static string NextTransformIndex()
        {
            var basePath = @"E:\Documents\Files\Pictures\Miscellaneous\Starfall\ImageTransforms";
            var existingDirectories = Directory.GetDirectories(basePath);
            var maxIndex = existingDirectories
                .Select(dir => Path.GetFileName(dir))
                .Where(name => int.TryParse(name, out _))
                .Select(int.Parse)
                .DefaultIfEmpty(-1)
                .Max();
            return (maxIndex + 1).ToString("D4");
        }

        private static ImageInfo GetImageInfo(string imagePath)
        {
            var info = Image.Identify(imagePath);
            return info;
        }
    }
}
