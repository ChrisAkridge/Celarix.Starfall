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
    "f03_floatsAreGoodAtMath",
    "f04_noEscapeFromInfiniteExpansions",
    "f05_butWellJustPickBinary",
    "f06_butWhyScientificNotation",
    "f07_rulesForMantissas",
    "f08_floatingPointIsScientificNotation",
    "f09_10_11_openTheWindow",
    "f13_impliedLeadingBits",   // No slide 12, we did so well in 9/10/11 that we don't need a 12
    "f14_15_specialExponents",
    "f16_lossOfPrecision",
    "s01_thisShouldBeProgrammable",
    "s02_introducingStarfall",
    "s03_noDSLs",
    "testSlide"
];
int currentSlideIndex = 14;

layoutEngine.AddSlide(new SlideFP_01_TitleSlide(ViewportWidth, ViewportHeight, measurementService), "f01_titleSlide");
layoutEngine.AddSlide(new SlideFP_02_IntegersAreGoodAtMath(ViewportWidth, ViewportHeight), "f02_integersAreGoodAtMath");
layoutEngine.AddSlide(new SlideFP_03_FloatsAreGoodAtMath(ViewportWidth, ViewportHeight), "f03_floatsAreGoodAtMath");
layoutEngine.AddSlide(new SlideFP_04_NoEscapeFromInfiniteExpansions(ViewportWidth, ViewportHeight), "f04_noEscapeFromInfiniteExpansions");
layoutEngine.AddSlide(new SlideFP_05_ButWellJustPickBinary(ViewportWidth, ViewportHeight), "f05_butWellJustPickBinary");
layoutEngine.AddSlide(new SlideFP_06_ButWhyScientificNotation(ViewportWidth, ViewportHeight), "f06_butWhyScientificNotation");
layoutEngine.AddSlide(new SlideFP_07_RulesForMantissas(ViewportWidth, ViewportHeight), "f07_rulesForMantissas");
layoutEngine.AddSlide(new SlideFP_08_FloatingPointIsScientificNotation(ViewportWidth, ViewportHeight), "f08_floatingPointIsScientificNotation");
layoutEngine.AddSlide(new SlideFP_09_10_11_OpenTheWindow(ViewportWidth, ViewportHeight), "f09_10_11_openTheWindow");
layoutEngine.AddSlide(new SlideFP_13_ImpliedLeadingBits(ViewportWidth, ViewportHeight), "f13_impliedLeadingBits");
layoutEngine.AddSlide(new SlideFP_14_15_SpecialExponents(ViewportWidth, ViewportHeight), "f14_15_specialExponents");
layoutEngine.AddSlide(new SlideFP_16_LossOfPrecision(ViewportWidth, ViewportHeight), "f16_lossOfPrecision");
layoutEngine.AddSlide(new SlideSF_01_ThisShouldBeProgrammable(ViewportWidth, ViewportHeight), "s01_thisShouldBeProgrammable");
layoutEngine.AddSlide(new SlideSF_02_IntroducingStarfall(ViewportWidth, ViewportHeight), "s02_introducingStarfall");
layoutEngine.AddSlide(new SlideSF_03_NoDSLs(ViewportWidth, ViewportHeight), "s03_noDSLs");
layoutEngine.AddSlide(new TestSlide(ViewportWidth, ViewportHeight), "testSlide");

tkTarget.KeyUp += TkTarget_KeyUp;

var firstSlide = slideNames[currentSlideIndex];
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