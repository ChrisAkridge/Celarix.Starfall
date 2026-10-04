using Celarix.Starfall.Atria;
using Celarix.Starfall.Presentation;
using Celarix.Starfall.Decks.FloatingPoint;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Initialization;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations
{
    internal sealed class PresentationRunner
    {
        private readonly PresentationInitializationArguments args;

        private readonly PresentationDefinition _presentation = FloatingPointPresentation.Create();
        private AtriaLayoutEngine _layoutEngine;
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
                for (int i = 0; i < _presentation.Slides.Count; i++)
                {
                    if (_presentation.Slides[i].DisplayName == currentSlideName)
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

            // Pick a monitor to display the presentation on
            var monitorInfos = MonitorInfoProvider.GetMonitorInfos();
            var monitorIndex = GetDesiredMonitorIndex(monitorInfos);

            var engineOptions = new PresentationEngineOptions
            {
                ErrorLevel = ErrorLevel.Display
            };

            _layoutEngine = new AtriaLayoutEngine(args.ViewportWidth, args.ViewportHeight);
            var tkTarget = new SkiaTkTarget(args.ViewportWidth,
                args.ViewportHeight,
                60,
                _presentation.Name,
                _layoutEngine,
                monitorIndex);
            tkTarget.KeyUp += TkTarget_KeyUp;

            _layoutEngine.Attach(tkTarget);
            _layoutEngine.Runtime!.Input = new WindowsPresenterInput();
            _layoutEngine.OnException += LayoutEngine_OnException;

            // Initialize and switch to the first slide
            InitializeAndSwitchToSlide(0);
            _layoutEngine.Start();
        }

        private int GetDesiredMonitorIndex(IReadOnlyList<SMonitorInfo> monitorInfos)
        {
            Console.WriteLine("Available monitors:");
            for (int i = 0; i < monitorInfos.Count; i++)
            {
                SMonitorInfo? monitorInfo = monitorInfos[i];
                Console.WriteLine($"\t{i}. ({monitorInfo.Width}x{monitorInfo.Height}) {monitorInfo.Name}");
            }
            Console.Write("Please select the monitor index to display the presentation on: ");

            int? chosenMonitorIndex = null;
            do
            {
                var input = Console.ReadLine();
                if (!int.TryParse(input, out int index) || index < 0 || index >= monitorInfos.Count)
                {
                    Console.WriteLine("Invalid input. Please enter a valid monitor index.");
                }
                else
                {
                    chosenMonitorIndex = index;
                }
            } while (chosenMonitorIndex == null);

            return chosenMonitorIndex.Value;
        }

        private void TkTarget_KeyUp(object? sender, KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right)
            {
                // Right: Advance the current slide
                var result = _layoutEngine.AdvanceCurrentSlide();

                if (result == SlideAdvanceResult.InternalStateChanged)
                {
                    WriteCurrentBeat();
                }
                else if (result == SlideAdvanceResult.CanAdvance)
                {
                    var nextSlideIndex = Math.Min((CurrentSlideIndex ?? 0) + 1, _presentation.Slides.Count - 1);
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
            if (slideIndex < 0 || slideIndex >= _presentation.Slides.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(slideIndex), "Slide index is out of range.");
            }

            if (slideIndex != CurrentSlideIndex)
            {
                // We're changing slides, so reset the rewind flag
                _rewindOccurred = false;
            }

            var slideDefinition = _presentation.Slides[slideIndex];
            Console.WriteLine($"INFO: Switching to slide {slideIndex}: {slideDefinition.DisplayName}");

            var slide = slideDefinition.Factory(_layoutEngine.Runtime!);

            // Remove and replace the current slide in the layout engine
            var currentSlideName = _layoutEngine.CurrentSlideName;
            if (currentSlideName != null)
            {
                _layoutEngine.RemoveSlide(currentSlideName);
            }

            _layoutEngine.AddSlide(slide, slideDefinition.DisplayName);
            _layoutEngine.SetCurrentSlide(slideDefinition.DisplayName);
            WriteCurrentBeat();
        }

        private void WriteCurrentBeat()
        {
            var slide = _layoutEngine.CurrentSlide;
            if (slide == null)
            {
                return;
            }

            Console.WriteLine($"INFO: {slide.Name} | beat {slide.BeatIndex}: {slide.CurrentBeat}");
            if (!string.IsNullOrWhiteSpace(slide.Notes))
            {
                Console.WriteLine($"NOTES: {slide.Notes}");
            }
        }

        private int AskUserToSwitchToSlide()
        {
            Console.WriteLine("Please select a slide to switch to by entering its number:");
            for (int i = 0; i < _presentation.Slides.Count; i++)
            {
                Console.WriteLine($"\t{i}: {_presentation.Slides[i].DisplayName}");
            }

            int? chosenSlideIndex = null;
            do
            {
                Console.Write("Input: ");
                var input = Console.ReadLine();
                if (int.TryParse(input, out int index) && index >= 0 && index < _presentation.Slides.Count)
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
