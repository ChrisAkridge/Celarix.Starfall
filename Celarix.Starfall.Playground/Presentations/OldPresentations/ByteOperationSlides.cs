using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Playground.AtriaTests.Operations;
using Celarix.Starfall.Presentation;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Celarix.Starfall.Playground.Presentations.OldPresentations
{
    internal class ByteOperationSlides
    {
        public static void Run()
        {
            RunOperation("01_Identity", (x, y) => x, "+x");
            RunOperation("02_Inverse", (x, y) => (byte)(-x), "-x");
            RunOperation("03_BitwiseNOT", (x, y) => (byte)(~x), "~x");
            RunOperation("04_LogicalNOT", (x, y) => (byte)(x == 0 ? 1 : 0), "!x");
            RunOperation("05_FactorialRounding", (x, y) => RoundingFactorial(x), "x! (round per step)");
            RunOperation("06_FactorialNonRounding", (x, y) => NonRoundingFactorial(x), "x! (no rounding)");
            RunOperation("07_Sumtorial", (x, y) => Sumtorial(x), "Sumtorial(x)");
            RunOperation("08_Square", (x, y) => (byte)Math.Pow(x, 2), "x²");
            RunOperation("09_Cube", (x, y) => (byte)Math.Pow(x, 3), "x³");
            RunOperation("10_Pow2", (x, y) => (byte)Math.Pow(2, x), "2ˣ");
            RunOperation("10_PowE", (x, y) => (byte)Math.Pow(Math.E, x), "eˣ");
            RunOperation("11_Pow10", (x, y) => (byte)Math.Pow(10, x), "10ˣ");
            RunOperation("12_Sqrt", (x, y) => (byte)Math.Sqrt(x), "√x");
            RunOperation("13_Cbrt", (x, y) => (byte)Math.Cbrt(x), "∛x");
            RunOperation("14_Log2", (x, y) => (byte)Math.Log2(Safe(x)), "log₂(x)");
            RunOperation("15_Ln", (x, y) => (byte)Math.Log(Safe(x), Math.E), "ln(x)");
            RunOperation("16_Log10", (x, y) => (byte)Math.Log10(Safe(x)), "log₁₀(x)");
            RunOperation("17_SinDegree", (x, y) => MapRange(Math.Sin(double.DegreesToRadians(x))), "sin(x) in degrees");
            RunOperation("18_CosDegree", (x, y) => MapRange(Math.Cos(double.DegreesToRadians(x))), "cos(x) in degrees");
            RunOperation("19_TanDegree", (x, y) => (byte)Math.Tan(double.DegreesToRadians(x)), "tan(x) in degrees");
            RunOperation("20_SinDegree", (x, y) => MapRange(Math.Sin(x)), "sin(x) in radians");
            RunOperation("21_CosDegree", (x, y) => MapRange(Math.Cos(x)), "cos(x) in radians");
            RunOperation("22_TanDegree", (x, y) => (byte)Math.Tan(x), "tan(x) in radians");
            RunOperation("23_Addition", (x, y) => (byte)(x + y), "x + y");
            RunOperation("24_Subtraction", (x, y) => (byte)(x - y), "x - y");
            RunOperation("24_f_Subtraction", (x, y) => (byte)(y - x), "y - x");
            RunOperation("25_Multiplication", (x, y) => (byte)(x * y), "x * y");
            RunOperation("26_Division", (x, y) => (byte)(x / Safe(y)), "x / y");
            RunOperation("26_f_Division", (x, y) => (byte)(y / Safe(x)), "y / x");
            RunOperation("27_Modulo", (x, y) => (byte)(x % Safe(y)), "x % y");
            RunOperation("27_f_Modulo", (x, y) => (byte)(y % Safe(x)), "y % x");
            RunOperation("28_BitwiseAND", (x, y) => (byte)(x & y), "x & y");
            RunOperation("29_BitwiseOR", (x, y) => (byte)(x | y), "x | y");
            RunOperation("30_BitwiseXOR", (x, y) => (byte)(x ^ y), "x ^ y");
            RunOperation("31_ShiftLeft", (x, y) => (byte)(x << (y % 8)), "x << y");
            RunOperation("31_f_ShiftLeft", (x, y) => (byte)(y << (x % 8)), "y << x");
            RunOperation("32_ShiftRight", (x, y) => (byte)(x >> (y % 8)), "x >> y");
            RunOperation("32_f_ShiftRight", (x, y) => (byte)(y >> (x % 8)), "y >> x");
            RunOperation("33_LogicalShiftRight", (x, y) => (byte)((sbyte)x >>> (y % 8)), "x >>> y");
            RunOperation("33_f_LogicalShiftRight", (x, y) => (byte)((sbyte)y >>> (x % 8)), "y >>> x");
            RunOperation("34_RotateLeft", (x, y) => (byte)((x << (y % 8)) | (x >> (8 - (y % 8)))), "x ROL y");
            RunOperation("34_f_RotateLeft", (x, y) => (byte)((y << (x % 8)) | (y >> (8 - (x % 8)))), "y ROL x");
            RunOperation("35_RotateRight", (x, y) => (byte)((x >> (y % 8)) | (x << (8 - (y % 8)))), "x ROR y");
            RunOperation("35_f_RotateRight", (x, y) => (byte)((y >> (x % 8)) | (y << (8 - (x % 8)))), "y ROR x");
            RunOperation("36_Equality", (x, y) => FromBool(x == y), "x == y");
            RunOperation("37_Inequality", (x, y) => FromBool(x != y), "x != y");
            RunOperation("38_GreaterThan", (x, y) => FromBool(x > y), "x > y");
            RunOperation("39_LessThan", (x, y) => FromBool(x < y), "x < y");
            RunOperation("40_GreaterThanOrEqual", (x, y) => FromBool(x >= y), "x >= y");
            RunOperation("41_LessThanOrEqual", (x, y) => FromBool(x <= y), "x <= y");
            RunOperation("42_CompareTo", (x, y) => (byte)x.CompareTo(y), "x.CompareTo(y)");
            RunOperation("43_Pow", (x, y) => (byte)Math.Pow(x, y), "x ^^ y");
            RunOperation("43_f_Pow", (x, y) => (byte)Math.Pow(y, x), "y ^^ x");
            RunOperation("44_Root", NthRoot, "yth root of x");
            RunOperation("44_f_Root", (x, y) => NthRoot(y, x), "xth root of y");
            RunOperation("45_Log", (x, y) => (byte)Math.Log(x, y >= 2 ? y : 2), "log_y(x)");
            RunOperation("45_f_Log", (x, y) => (byte)Math.Log(y, x >= 2 ? x : 2), "log_x(y)");
            RunOperation("46_MaterialNonImplication", BitwiseMaterialNonImplication, "x & ~y (truth 2)");
            RunOperation("47_ConverseNonImplication", BitwiseConverseNonImplication, "~x & y (truth 4)");
            RunOperation("48_NOR", BitwiseNOR, "~(x | y) (truth 8)");
            RunOperation("49_XNOR", BitwiseXNOR, "~(x ^ y) (truth 9)");
            RunOperation("50_ConverseImplication", BitwiseConverseImplication, "x | ~y (truth 11)");
            RunOperation("51_MaterialConditional", BitwiseMaterialConditional, "~x | y (truth 13)");
            RunOperation("52_NAND", BitwiseNAND, "~(x & y) (truth 14)");
            RunOperation("53_PopCount", (x, y) => (byte)BitOperations.PopCount(x), "popcount(x)");
            RunOperation("54_LeadingZeroCount", (x, y) => (byte)(BitOperations.LeadingZeroCount(x) - 24), "lzcnt8(x)");
            RunOperation("55_TrailingZeroCount", (x, y) => x == 0 ? (byte)8 : (byte)BitOperations.TrailingZeroCount(x), "tzcnt8(x)");
            RunOperation("56_Min", Math.Min, "min(x,y)");
            RunOperation("57_Max", Math.Max, "max(x,y)");
            RunOperation("58_AbsDiff", (x, y) => (byte)Math.Abs(x - y), "|x-y|");
            RunOperation("59_Average", (x, y) => (byte)((x + y) / 2), "avg(x,y)");
            RunOperation("60_GCD", Gcd, "gcd(x,y)");
            RunOperation("61_BitClear", (x, y) => (byte)(x & ~y), "x & ~y");
            RunOperation("63_LowNibble", (x, y) => (byte)(x & 0x0F), "x & 0x0F");
            RunOperation("64_HighNibble", (x, y) => (byte)(x >> 4), "x >> 4");
            RunOperation("65_SwapNibbles", (x, y) => (byte)((x << 4) | (x >> 4)), "swap nibbles");
            RunOperation("66_ReverseBits", (x, y) => ReverseBits8(x), "bitreverse(x)");
            RunOperation("67_Quadrant", (x, y) => Quadrant(x), "quadrant(x)");
        }

        private static void RunOperation(string path, Func<byte, byte, byte> transform, string transformText)
        {
            Console.WriteLine($"Running {path}...");
            var engineOptions = new PresentationEngineOptions
            {
                ErrorLevel = ErrorLevel.Display
            };
            var layoutEngine = new AtriaLayoutEngine(1920, 1080);
            using var pngTarget = new SkiaPngTarget(new Rendering.Models.SkiaPngTargetOptions
            {
                Width = 1920,
                Height = 1080,
                FramesPerSecond = 60,
                OutputPath = Path.Combine(@"E:\Documents\Files\Pictures\Miscellaneous\Starfall\ByteOperations", path)
            });
            layoutEngine.SetRenderTarget(pngTarget);
            var measurementService = new Rendering.MeasurementService(pngTarget);
            layoutEngine.MeasurementService = measurementService;

            var byteOperationsSlide = new ByteOperationSlide(transform, transformText, 1920, 1080, sizeMultiplier: 1.5d);
            layoutEngine.AddSlide(byteOperationsSlide, "Byte Operations");
            layoutEngine.SetCurrentSlide("Byte Operations");

            var frameNumber = 0;
            while (!byteOperationsSlide.Completed)
            {
                Console.WriteLine($"\tRendering frame {frameNumber++}");
                layoutEngine.OnFrameRequested(1.0 / 60);
            }
        }

        private static byte Safe(byte value) => (byte)(value == 0 ? 1 : value);
        private static byte FromBool(bool value) => value ? (byte)255 : (byte)0;

        private static byte MapRange(double value)
        {
            // Map -1.0 to -128 and +1.0 to 127
            return (byte)((value + 1.0) * 127.5);
        }

        private static byte RoundingFactorial(byte value)
        {
            byte result = 1;
            while (value > 0)
            {
                result = (byte)(result * value);
                if (result == 0)
                {
                    return 0;
                }
                value--;
            }
            return result;
        }

        private static byte NonRoundingFactorial(byte value)
        {
            BigInteger result = 1;
            while (value > 0)
            {
                result *= value;
                value--;
            }
            return (byte)(result % 256);
        }

        private static byte Sumtorial(byte value)
        {
            var a = value + 1;
            var b = value * a;
            var c = b / 2;
            return (byte)c;
        }

        private static byte NthRoot(byte x, byte y)
        {
            // yth root of x
            var power = 1d / Safe(y);
            return (byte)Math.Pow(x, power);
        }

        // Logical connectives
        // For inputs (0, 0), (0, 1), (1, 0), and (1, 1)
        // 0 0 0 0: False
        // 0 0 0 1: AND
        // 0 0 1 0 : Material Non-Implication
        private static byte BitwiseMaterialNonImplication(byte x, byte y) => (byte)(x & ~y);
        // 0 0 1 1: <just X>
        // 0 1 0 0: Converse Non-Implication
        private static byte BitwiseConverseNonImplication(byte x, byte y) => (byte)(~x & y);
        // 0 1 0 1: <just Y>
        // 0 1 1 0: XOR
        // 0 1 1 1: OR
        // 1 0 0 0: NOR
        private static byte BitwiseNOR(byte x, byte y) => (byte)~(x | y);
        // 1 0 0 1: XNOR
        private static byte BitwiseXNOR(byte x, byte y) => (byte)~(x ^ y);
        // 1 0 1 0: NOT Y
        // 1 0 1 1: Converse Implication
        private static byte BitwiseConverseImplication(byte x, byte y) => (byte)(x | ~y);
        // 1 1 0 0: NOT X
        // 1 1 0 1: Material Conditional
        private static byte BitwiseMaterialConditional(byte x, byte y) => (byte)(~x | y);
        // 1 1 1 0: NAND
        private static byte BitwiseNAND(byte x, byte y) => (byte)~(x & y);
        // 1 1 1 1: True

        private static byte Gcd(byte x, byte y)
        {
            while (y != 0)
            {
                var temp = y;
                y = (byte)(x % y);
                x = temp;
            }
            return x;
        }

        private static byte ReverseBits8(byte value)
        {
            byte result = 0;
            for (int i = 0; i < 8; i++)
            {
                result <<= 1;
                result |= (byte)(value & 1);
                value >>= 1;
            }
            return result;
        }

        private static byte Quadrant(byte value)
        {
            var y = value >> 4;
            var x = value & 0x0F;

            var interleave = 0;
            interleave |= ((y & 0b1000) >> 3) << 7;
            interleave |= ((x & 0b1000) >> 3) << 6;
            interleave |= ((y & 0b0100) >> 2) << 5;
            interleave |= ((x & 0b0100) >> 2) << 4;
            interleave |= ((y & 0b0010) >> 1) << 3;
            interleave |= ((x & 0b0010) >> 1) << 2;
            interleave |= ((y & 0b0001) >> 0) << 1;
            interleave |= ((x & 0b0001) >> 0) << 0;
            return (byte)interleave;
        }
    }
}
