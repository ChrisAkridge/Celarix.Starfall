using Celarix.Starfall.Atria;
using Celarix.Starfall.Decks.Playground.AtriaTests;
using Celarix.Starfall.Decks.Playground.AtriaTests.CanonicalDecomposition;
using Celarix.Starfall.Decks.Playground.AtriaTests.Operations;
using Celarix.Starfall.Decks.Playground.MathFun;
using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Decks.Playground
{
    /// <summary>
    /// Every slide from the Playground in one deck, for trying out Starfall features in the harness.
    /// </summary>
    public static class PlaygroundPresentation
    {
        private const string SamplePhotoPath = "Assets/Images/3317968666_f46dbaac72_o_cropped.jpg";
        private const string SmallSampleImagePath = "Assets/Images/cssPositioning.png";

        public static PresentationDefinition Create() => new()
        {
            Name = "Playground",
            Folder = "Testing",
            Description = "The Playground's test slides, one after another.",
            Slides =
            [
                new("Sunday Night Lights", runtime => new SundayNightLights(runtime, runtime.ViewportSize))
                {
                    Description = "Sliding list with an arrow element."
                },
                new("Square Root Search", runtime => new SquareRootSearchSlide(runtime, runtime.ViewportSize))
                {
                    Description = "Binary search for a square root on a layered slide."
                },
                new("Math Fun", runtime => new MathFunSlide(runtime, runtime.ViewportSize))
                {
                    Description = "Libra math typesetting."
                },
                new("Stats", runtime => new StatsSlide(runtime, runtime.ViewportSize))
                {
                    Description = "Bar chart display."
                },
                new("Graph", runtime => new GraphSlide(runtime, runtime.ViewportSize))
                {
                    Description = "Force-directed graph layout."
                },
                new("Time Progress", runtime => new TimeProgressSlide(runtime, runtime.ViewportSize))
                {
                    Description = "Progress through the current day, week, month and year."
                },
                new("Gigasecond", runtime => new GigasecondSlide(runtime, runtime.ViewportSize))
                {
                    Description = "Countdown panels. Originally sized as a 1920×188 banner."
                },
                new("Thanksgiving Contradiction", runtime => new ThanksgivingContradictionSlide(runtime, runtime.ViewportSize))
                {
                    Description = "Custom diagram element."
                },
                new("Meijer Employment Intro", runtime => new MeijerEmploymentIntro(runtime, runtime.ViewportSize))
                {
                    Description = "Day-type display element."
                },
                new("Byte Operation: x & y", runtime => new ByteOperationSlide((x, y) => (byte)(x & y), "x & y",
                    (int)runtime.ViewportSize.Width, (int)runtime.ViewportSize.Height, sizeMultiplier: 1d, runtime, runtime.ViewportSize))
                {
                    Description = "A 256-value grid sweeping through every y for a byte operation."
                },
                new("Short Operation: quadrant(x)", runtime => new ShortOperationSlide((x, y) => Quadrant(x), "quadrant(x)",
                    (int)runtime.ViewportSize.Width, (int)runtime.ViewportSize.Height, sizeMultiplier: 1d, runtime, runtime.ViewportSize))
                {
                    Description = "A 65,536-value grid for a 16-bit operation."
                },
                new("Canonical Decomposition", runtime => new CanonicalDecompositionSlide(
                    AskForImage(runtime, "Pick an image to decompose (Cancel uses a sample photo).", SamplePhotoPath),
                    runtime, runtime.ViewportSize))
                {
                    Description = "Zoomable camera over an image. Asks for an image when it starts."
                },
                new("Image Transform", runtime =>
                {
                    var center = new SPointF(runtime.ViewportSize.Width / 2, runtime.ViewportSize.Height / 2);
                    return new ImageTransformSlide(
                        AskForImage(runtime, "Pick a small image to transform (Cancel uses a sample).", SmallSampleImagePath),
                        (int)runtime.ViewportSize.Width, (int)runtime.ViewportSize.Height,
                        // Point-reflect through the center, which reverses the pixel order like the Playground's ~index transform.
                        position => new SPointF(2 * center.X - position.X, 2 * center.Y - position.Y),
                        color => color,
                        runtime, runtime.ViewportSize);
                })
                {
                    Description = "Every pixel of an image animates to a new position. Asks for an image when it starts; keep it small, since each pixel is drawn separately."
                }
            ]
        };

        private static string AskForImage(AtriaRuntime runtime, string prompt, string fallbackPath) =>
            runtime.Input.AskForFile(prompt) is { Length: > 0 } path ? path : Path.Combine(AppContext.BaseDirectory, fallbackPath);

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
    }
}
