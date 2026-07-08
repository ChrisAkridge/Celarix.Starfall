using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Presentation;
using Celarix.Starfall.Presentations.FloatingPoint;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Targets;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations
{
    internal sealed class PresentationRunner
    {
        private class SlideFactory
        {
            public string Name { get; set; }
            public Func<AtriaSlide> Factory { get; set; }

            public SlideFactory(string name, Func<AtriaSlide> factory)
            {
                Name = name;
                Factory = factory;
            }
        }

        private readonly PresentationInitializationArguments args;

        private readonly List<SlideFactory> _slideFactories = new();
        private AtriaLayoutEngine _layoutEngine;
        private MeasurementService _measurementService;
        private bool _rewindOccurred;

        // A hackish, terrible way to do this, but O(N) is basically O(1) for N = like 15
        private int? CurrentSlideIndex
        {
            get
            {
                var currentSlideName = _layoutEngine.CurrentSlideName;
                if (currentSlideName == null)
                {
                    return null;
                }
                for (int i = 0; i < _slideFactories.Count; i++)
                {
                    if (_slideFactories[i].Name == currentSlideName)
                    {
                        return i;
                    }
                }
                return -1;
            }
        }

        public PresentationRunner(PresentationInitializationArguments args)
        {
            this.args = args;
        }

        public void Run()
        {
            Console.WriteLine("INFO: Running presentation...");
            var engineOptions = new PresentationEngineOptions
            {
                ErrorLevel = ErrorLevel.Display
            };

            _layoutEngine = new AtriaLayoutEngine(args.ViewportWidth, args.ViewportHeight);
            var tkTarget = new SkiaTkTarget(args.ViewportWidth, args.ViewportHeight, 60, "Floating Point Numbers, Visualized", _layoutEngine);
            tkTarget.KeyUp += TkTarget_KeyUp;

            _layoutEngine.SetRenderTarget(tkTarget);
            _measurementService = new MeasurementService(tkTarget);
            _layoutEngine.MeasurementService = _measurementService;
            _layoutEngine.OnException += LayoutEngine_OnException;

            // Register slide factories here
            _slideFactories.Add(new SlideFactory("FP Title", () => new SlideFP_01_TitleSlide(args.ViewportWidth, args.ViewportHeight, _measurementService)));
            _slideFactories.Add(new SlideFactory("FP Integers are Good at Math", () => new SlideFP_02_IntegersAreGoodAtMath(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("FP Floats are Good at Math", () => new SlideFP_03_FloatsAreGoodAtMath(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("FP No Escape from Infinite Expansions", () => new SlideFP_04_NoEscapeFromInfiniteExpansions(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("FP But We'll Just Pick Binary", () => new SlideFP_05_ButWellJustPickBinary(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("FP But Why Scientific Notation?", () => new SlideFP_06_ButWhyScientificNotation(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("FP Rules for Mantissas", () => new SlideFP_07_RulesForMantissas(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("FP Floating Point is Scientific Notation", () => new SlideFP_08_FloatingPointIsScientificNotation(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("FP Open the Window", () => new SlideFP_09_10_11_OpenTheWindow(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("FP Implied Leading Bits", () => new SlideFP_13_ImpliedLeadingBits(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("FP Special Exponents", () => new SlideFP_14_15_SpecialExponents(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("FP Loss of Precision", () => new SlideFP_16_LossOfPrecision(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("SF This Should Be Programmable", () => new SlideSF_01_ThisShouldBeProgrammable(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("SF Introducing Starfall", () => new SlideSF_02_IntroducingStarfall(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("SF No DSLs", () => new SlideSF_03_NoDSLs(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("SF No Absolute Positioning", () => new SlideSF_04_NoAbsolutePositioning(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("SF Binary Drawing Example", () => new SlideSF_05_BinaryDrawing(args.ViewportWidth, args.ViewportHeight)));
            _slideFactories.Add(new SlideFactory("SF Thank You", () => new SlideSF_06_ThankYou(args.ViewportWidth, args.ViewportHeight)));

            // Initialize and switch to the first slide
            InitializeAndSwitchToSlide(0);
            _layoutEngine.Start();
        }

        private void TkTarget_KeyUp(object? sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right)
            {
                // Right: Advance the current slide
                var result = _layoutEngine.AdvanceCurrentSlide();

                if (result == SlideAdvanceResult.CanAdvance)
                {
                    var nextSlideIndex = Math.Min((CurrentSlideIndex ?? 0) + 1, _slideFactories.Count - 1);
                    InitializeAndSwitchToSlide(nextSlideIndex);
                }
            }
            else if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left)
            {
                // Handle rewind a little differently, since most slides aren't fully set up for rewinding,
                // and, frankly, that's a little more effort than I think is needed. The first Left press
                // reinitializes the current slide fully, resetting it to its initial state. the second
                // Left press actually rewinds to the previous slide.
                if (!_rewindOccurred)
                {
                    InitializeAndSwitchToSlide(CurrentSlideIndex ?? 0);
                    _rewindOccurred = true;
                }
                else
                {
                    var previousSlideIndex = Math.Max((CurrentSlideIndex ?? 0) - 1, 0);
                    InitializeAndSwitchToSlide(previousSlideIndex);
                }
            }
            else if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.R)
            {
                // R: Display the slide picker and get the answer from the user
                var chosenSlide = AskUserToSwitchToSlide();
                _rewindOccurred = false;  // Reset the rewind flag since we're switching slides... even if we're not (i.e. the user picked the current slide)
                InitializeAndSwitchToSlide(chosenSlide);
            }
        }

        private void LayoutEngine_OnException(object? sender, Exception e)
        {
            // Handle exceptions from the layout engine
            Console.ForegroundColor = ConsoleColor.Yellow;  // looks better on a blue console background
            Console.WriteLine($"Layout Engine Exception: {e.Message}");
            Console.ForegroundColor = ConsoleColor.White;

            // Try to reinitialize the current slide and switch to it again
            InitializeAndSwitchToSlide(CurrentSlideIndex ?? 0);
        }

        // Orchestration methods
        private void InitializeAndSwitchToSlide(int slideIndex)
        {
            if (slideIndex < 0 || slideIndex >= _slideFactories.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(slideIndex), "Slide index is out of range.");
            }

            if (slideIndex != CurrentSlideIndex)
            {
                // We're changing slides, so reset the rewind flag
                _rewindOccurred = false;
            }

            Console.WriteLine($"INFO: Switching to slide {slideIndex}: {_slideFactories[slideIndex].Name}");

            var slideFactory = _slideFactories[slideIndex];
            var slide = slideFactory.Factory();

            // Remove and replace the current slide in the layout engine
            var currentSlideName = _layoutEngine.CurrentSlideName;
            if (currentSlideName != null)
            {
                _layoutEngine.RemoveSlide(currentSlideName);
            }

            _layoutEngine.AddSlide(slide, slideFactory.Name);
            _layoutEngine.SetCurrentSlide(slideFactory.Name);
        }

        private int AskUserToSwitchToSlide()
        {
            Console.WriteLine("Please select a slide to switch to by entering its number:");
            for (int i = 0; i < _slideFactories.Count; i++)
            {
                Console.WriteLine($"\t{i}: {_slideFactories[i].Name}");
            }

            int? chosenSlideIndex = null;
            do
            {
                Console.Write("Input: ");
                var input = Console.ReadLine();
                if (int.TryParse(input, out int index) && index >= 0 && index < _slideFactories.Count)
                {
                    chosenSlideIndex = index;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter a valid slide number.");
                }
            } while (chosenSlideIndex == null);

            return chosenSlideIndex.Value;
        }
    }
}
