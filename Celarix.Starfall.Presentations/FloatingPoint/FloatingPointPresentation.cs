using Celarix.Starfall.Atria;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal static class FloatingPointPresentation
    {
        public static PresentationDefinition Create() => new()
        {
            Name = "Floating Point Numbers, Visualized",
            Folder = "Talks",
            Description = "KYOSS talk, July 2026: how IEEE 754 floats work, then an introduction to Starfall.",
            Slides =
            [
                new("FP Title", runtime => new SlideFP_01_TitleSlide(runtime, runtime.ViewportSize))
                {
                    Description = "Title card for the talk."
                },
                new("FP Integers are Good at Math", runtime => new SlideFP_02_IntegersAreGoodAtMath(runtime, runtime.ViewportSize))
                {
                    Description = "Integer arithmetic problems that come out exact."
                },
                new("FP Floats are Good at Math", runtime => new SlideFP_03_FloatsAreGoodAtMath(runtime, runtime.ViewportSize))
                {
                    Description = "Float arithmetic that mostly works, until 0.1 + 0.2."
                },
                new("FP No Escape from Infinite Expansions", runtime => new SlideFP_04_NoEscapeFromInfiniteExpansions(runtime, runtime.ViewportSize))
                {
                    Description = "Long division showing fractions that never terminate."
                },
                new("FP But We'll Just Pick Binary", runtime => new SlideFP_05_ButWellJustPickBinary(runtime, runtime.ViewportSize))
                {
                    Description = "Decimal place values and exponents, then the move to binary."
                },
                new("FP But Why Scientific Notation?", runtime => new SlideFP_06_ButWhyScientificNotation(runtime, runtime.ViewportSize))
                {
                    Description = "Very large and very small numbers, and why scientific notation helps."
                },
                new("FP Rules for Mantissas", runtime => new SlideFP_07_RulesForMantissas(runtime, runtime.ViewportSize))
                {
                    Description = "Equivalent ways to write one number, and which mantissa is the normalized one."
                },
                new("FP Choosing Bit Allocation", runtime => new SlideFP_07_5_ChoosingBitAllocation(runtime, runtime.ViewportSize))
                {
                    Description = "Splitting 32 bits between sign, exponent, and mantissa."
                },
                new("FP Exponent as an Unsigned Integer", runtime => new SlideFP_07_6_ExponentAsUnsignedInteger(runtime, runtime.ViewportSize))
                {
                    Description = "Storing the exponent as a biased unsigned integer."
                },
                new("FP Floating Point is Scientific Notation", runtime => new SlideFP_08_FloatingPointIsScientificNotation(runtime, runtime.ViewportSize))
                {
                    Description = "A float's bits read as binary scientific notation."
                },
                new("FP Open the Window", runtime => new SlideFP_09_10_11_OpenTheWindow(runtime, runtime.ViewportSize))
                {
                    Description = "The bit window: building a target value bit by bit, with values from the audience."
                },
                new("FP Implied Leading Bits", runtime => new SlideFP_13_ImpliedLeadingBits(runtime, runtime.ViewportSize))
                {
                    Description = "Why the leading 1 doesn't need to be stored."
                },
                new("FP Special Exponents", runtime => new SlideFP_14_15_SpecialExponents(runtime, runtime.ViewportSize))
                {
                    Description = "Zero, subnormals, infinities and NaN from the reserved exponents."
                },
                new("FP Loss of Precision", runtime => new SlideFP_16_LossOfPrecision(runtime, runtime.ViewportSize))
                {
                    Description = "How precision thins out as numbers grow."
                },
                new("SF This Should Be Programmable", runtime => new SlideSF_01_ThisShouldBeProgrammable(runtime, runtime.ViewportSize))
                {
                    Description = "Transition from the float talk to Starfall."
                },
                new("SF Introducing Starfall", runtime => new SlideSF_02_IntroducingStarfall(runtime, runtime.ViewportSize))
                {
                    Description = "Starfall: code-first presentations."
                },
                new("SF No DSLs", runtime => new SlideSF_03_NoDSLs(runtime, runtime.ViewportSize))
                {
                    Description = "Why Starfall slides are plain C# rather than a DSL."
                },
                new("SF No Absolute Positioning", runtime => new SlideSF_04_NoAbsolutePositioning(runtime, runtime.ViewportSize))
                {
                    Description = "Relative layout instead of PowerPoint/Word/CSS absolute positioning."
                },
                new("SF Binary Drawing Example", runtime => new SlideSF_05_BinaryDrawing(runtime, runtime.ViewportSize))
                {
                    Description = "Drawing arbitrary files as colored binary images."
                },
                new("SF Thank You", runtime => new SlideSF_06_ThankYou(runtime, runtime.ViewportSize))
                {
                    Description = "Closing slide."
                }
            ]
        };
    }
}
