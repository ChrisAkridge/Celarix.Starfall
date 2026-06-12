// Floating Point Numbers, Visualized
// for the Kentucky Open Source Society
// July 2026


// Console Controller
//	Modes:
//		-Floating Point Path: Dark blue background
//        - Starfall Path: Dark teal background
//	Commands:
//		- Right arrow (or whatever the Logitech presentation clicker uses): Advance to the next slide/advance within current slide
//		- Left arrow: Go back to the previous slide/go back within current slide

using Celarix.Starfall;
using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Presentation;
using Celarix.Starfall.Presentations.FloatingPoint;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Targets;

var engineOptions = new PresentationEngineOptions
{
    ErrorLevel = ErrorLevel.Display
};

const int ViewportWidth = 1280;
const int ViewportHeight = 720;

var layoutEngine = new AtriaLayoutEngine(ViewportWidth, ViewportHeight);
var tkTarget = new SkiaTkTarget(ViewportWidth, ViewportHeight, 60, "Floating Point Numbers, Visualized", layoutEngine);


layoutEngine.SetRenderTarget(tkTarget);
var measurementService = new MeasurementService(tkTarget);
layoutEngine.MeasurementService = measurementService;

string[] slideNames = [
    "f01_titleSlide",
    "f02_integersAreGoodAtMath",
    "f03_floatsAreGoodAtMath"
];
int currentSlideIndex = 0;

layoutEngine.AddSlide(new SlideFP_01_TitleSlide(ViewportWidth, ViewportHeight, measurementService), "f01_titleSlide");
layoutEngine.AddSlide(new SlideFP_02_IntegersAreGoodAtMath(ViewportWidth, ViewportHeight), "f02_integersAreGoodAtMath");
layoutEngine.AddSlide(new SlideFP_03_FloatsAreGoodAtMath(ViewportWidth, ViewportHeight), "f03_floatsAreGoodAtMath");

tkTarget.KeyUp += TkTarget_KeyUp;

var firstSlide = slideNames[2];
layoutEngine.SetCurrentSlide(firstSlide);
layoutEngine.Start();

void TkTarget_KeyUp(object? sender, OpenTK.Windowing.Common.KeyboardKeyEventArgs e)
{
    SlideAdvanceResult? result = null;
    if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right)
    {
        result = layoutEngine.AdvanceCurrentSlide();
    }
    else if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left)
    {
        result = layoutEngine.RewindCurrentSlide();
    }

    if (result != null && result != SlideAdvanceResult.InternalStateChanged)
    {
        if (result == SlideAdvanceResult.CanAdvance && currentSlideIndex < slideNames.Length - 1)
        {
            currentSlideIndex++;
            layoutEngine.SetCurrentSlide(slideNames[currentSlideIndex]);
        }
        else if (result == SlideAdvanceResult.CanRewind && currentSlideIndex > 0)
        {
            currentSlideIndex--;
            layoutEngine.SetCurrentSlide(slideNames[currentSlideIndex]);
        }
    }
}