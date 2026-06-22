using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideFP_06_ButWhyScientificNotation : AtriaSlide
    {
        private const string InitialPlanckLengthText = "0.00000000000000000000000000000000001616255";

        private int _state;
        private AnimationContext _animationContext;

        public SlideFP_06_ButWhyScientificNotation(int width, int height) : base(width, height)
        {
            _animationContext = new AnimationContext();
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.FloatingPointBackground;

            // Avogadro's constant
            // 6.02214076×10^23 mol^−1
            var bigNumberElement = new TextBlock("#bigNumber")
            {
                Text = "602214076000000000000000",
                FontFamily = "Consolas",
                FontSize = 72f,
                Color = SColor.White
            };

            var basisLine = new BasisLine(TopCenter, BottomCenter);
            var bigNumberAnchor = new BasisPoint(basisLine.SplitAndTakeLeft(1f / 3f).Center, "#bigNumberAnchor");
            bigNumberElement.AnchorCenterTo(bigNumberAnchor);

            // Planck length in meters
            // 1.616255×10^−35 m
            var smallNumberElement = new TextBlock("#smallNumber")
            {
                Text = InitialPlanckLengthText,
                FontFamily = "Consolas",
                FontSize = 48f,
                Color = SColor.White
            };
            var smallNumberAnchor = new BasisPoint(basisLine.SplitAndTakeRight(2f / 3f).Center, "#smallNumberAnchor");
            smallNumberElement.AnchorCenterTo(smallNumberAnchor);

            Add([bigNumberElement, bigNumberAnchor, smallNumberElement, smallNumberAnchor]);
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            _animationContext.Update(AtriaLayoutEngine.GlobalFrameNumber);
        }

        private Action<TextBlock, TextBlock>[] GetStateMethods()
        {
            return [
                State1,
                State2,
                State3,
                State4,
                State5,
                State6,
                State7,
                State8,
                State9,
                State10,
                State11,
                State12,
                State13,
                State14,
                State15,
                State16,
                State17,
                State18,
                State19,
                State20,
                State21,
                State22,
                State23,
                State24,
                State25,
                State26,
                State27,
                State28,
                State29,
                State30,
                State31,
                State32,
                State33,
                State34,
                State35,
                State36,
                State37,
                State38
            ];
        }

        public override SlideAdvanceResult Advance()
        {
            var bigNumberElement = (TextBlock)Query("#bigNumber").Single();
            var smallNumberElement = (TextBlock)Query("#smallNumber").Single();

            Action<TextBlock, TextBlock>[] stateMethods = GetStateMethods();

            if (_state < stateMethods.Length)
            {
                stateMethods[_state](bigNumberElement, smallNumberElement);
                _state++;
                return SlideAdvanceResult.InternalStateChanged;
            }
            else
            {
                return SlideAdvanceResult.CanAdvance;
            }
        }

        public override SlideAdvanceResult Rewind()
        {
            var bigNumberElement = (TextBlock)Query("#bigNumber").Single();
            var smallNumberElement = (TextBlock)Query("#smallNumber").Single();

            Action<TextBlock, TextBlock>[] stateMethods = GetStateMethods();

            if (_state > 0)
            {
                _state--;
                stateMethods[_state](bigNumberElement, smallNumberElement);
                return SlideAdvanceResult.InternalStateChanged;
            }
            else
            {
                return SlideAdvanceResult.CanRewind;
            }
        }

        // "Now, there is a common problem we face in science and mathematics, a lot of the numbers
        // we find experimentally or have to work with end up being very large..."
        private void State1(TextBlock bigNumberElement, TextBlock smallNumberElement) => _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(
                AnimationContext.SecondsToFrames(0.5d),
                p => bigNumberElement.Opacity = p
            ));

        // "...or very small."
        private void State2(TextBlock bigNumberElement, TextBlock smallNumberElement) => _animationContext.ScheduleAnimation(FixedDurationAnimation.StartNow(
                AnimationContext.SecondsToFrames(0.5d),
                p => smallNumberElement.Opacity = p
            ));

        // "These numbers take up lots of space, are hard to mentally compare, and lead to all kinds
        // errors. How many zeroes are in the number on the bottom? They tend to run together very easily.
        // So mathematicians came up with a solution to this problem, scientific notation. We'll start with
        // the bigger number on top. What we do..."
        private void State3(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "60221407600000000000000 × 10";

        // "...is a balanced sort of rewriting. We divide the big number by 10, but we add a multiplication
        // by 10 to compensate. This is similar to doing the same thing to both sides of an equation in algebra,
        // but since there's no equals sign in sight, we end up having to do something that cancels itself out.
        // We could have, instead, subtracted 1 from the big number and added 1 to it, which would have kept balance.
        // Then we..."

        private void State4(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "6022140760000000000000 × 10 × 10";

        // "...do it again. We do it again and again..."
        private void State5(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "602214076000000000000 × 10 × 10 × 10";
        private void State6(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "60221407600000000000 × 10 × 10 × 10 × 10";
        private void State7(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "6022140760000000000 × 10 × 10 × 10 × 10 × 10";
        private void State8(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "602214076000000000 × 10 × 10 × 10 × 10 × 10 × 10";
        private void State9(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "60221407600000000 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        private void State10(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "6022140760000000 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        private void State11(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "602214076000000 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        private void State12(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "60221407600000 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        private void State13(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "6022140760000 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        private void State14(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "602214076000 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        // "...over and over..."
        private void State15(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "60221407600 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        private void State16(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "6022140760 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        private void State17(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "602214076 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        // "...even letting the big number get a decimal part..."
        private void State18(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "60221407.6 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        private void State19(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "6022140.76 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        private void State20(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "602214.076 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        private void State21(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "60221.4076 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        private void State22(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "6022.14076 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        private void State23(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "602.214076 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        private void State24(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "60.2214076 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        // "...until, finally..."
        private void State25(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "6.02214076 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10 × 10";
        // "...we reach the defined stopping point. The big number has been reduced to a single digit
        // above the decimal point and a huge chain of multiplies by 10 to the right... okay, yeah, this
        // still looks pretty unwieldy and nobody writes numbers like this, either. We take advantages
        // of exponentiation."
        private void State26(TextBlock bigNumberElement, TextBlock smallNumberElement) => smallNumberElement.Text = "aⁿ = a × a × ... × a, n times";
        // "Exponentiation is a shorthand form of repeated multiplication. This lets us collapse this
        // awful chain of multiplies by 10 into a single neat form..."
        private void State27(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "6.02214076 × 10²³";
        // "Much better. We just combined 23 different multiplies by 10 into a single multiplication by 10 raised to the power of 23.
        // And it gives you a bonus - since the exponents come up so much, you quickly
        // get a sense of scale. An exponent of 3 is something in the thousands, 6 means something in
        // the millions, 12 is in the trillions, and so forth. A number is bigger than another if the
        // power, the little number up there, is higher. If they're the same, only then do you have
        // to look at the part on the left. Oh, and also..."
        private void State28(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "6.022 × 10²³";
        // "...usually, when it comes to really big numbers, their absolute size matters more than
        // the specifics of every digit. So it's usually fine to round the number on the left down, saving
        // even more space. This also works for small numbers..."
        private void State29(TextBlock bigNumberElement, TextBlock smallNumberElement) => smallNumberElement.Text = InitialPlanckLengthText;
        // "...where, instead of dividing the left by 10 and adding a multiply by 10 to keep balance, we
        // go in reverse. Multiply the left by 10 and divide by 10 to keep balance."
        private void State30(TextBlock bigNumberElement, TextBlock smallNumberElement) => smallNumberElement.Text = "0.0000000000000000000000000000000001616255 ÷ 10";
        // "I won't make you sit through that again, here's the final result, when we move that 1 all
        // the way to the left by repeatedly multiplying by 10 and dividing by 10 to keep balance."
        private void State31(TextBlock bigNumberElement, TextBlock smallNumberElement)
        {
            smallNumberElement.Text = "1.616255 ÷ 10³⁵";
            smallNumberElement.FontSize = 72f;
            smallNumberElement.Size = MeasurementService.MeasureText(smallNumberElement.Text, new SFontFamily("Consolas", 72f));
        }

        // "But we do have one more neat trick. Multiplication and division are secretly the same thing,
        // just in different directions. We currently have one point six one up there divided by a huge
        // power of ten, but we can make it look exactly the same as the first example by just..."
        private void State32(TextBlock bigNumberElement, TextBlock smallNumberElement) => smallNumberElement.Text = "1.616255 × 10⁻³⁵";
        // "Multiplying it by a negative power of ten? Yep! Think about it like this. Why did we choose 10?
        // Because, in our base-10 counting system, we can multiply and divide numbers by 10 just by
        // shifting digits left or right and adding zeroes if needed. Let's start with one thousand..."
        private void State33(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "1000 = 10 × 10 × 10 = 10³";
        // "...and see that when we divide by 10, we see that exponent go down by 1..."
        private void State34(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "1000 ÷ 10 = 100 = 10 × 10 = 10²";
        // "...and dividing by 10 again gets us down to a power of 1..."
        private void State35(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "100 ÷ 10 = 10 = 10¹";
        // "...and we can keep going..."
        private void State36(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "10 ÷ 10 = 1 = 10⁰";
        // "...into the negatives."
        private void State37(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "1 ÷ 10 = 0.1 = 10⁻¹";
        // "Multiplication and division on numbers becomes addition and subtraction on powers. We just
        // subtracted 1 from the power and dropped by a factor of 10 each time. Now we can do something like..."
        private void State38(TextBlock bigNumberElement, TextBlock smallNumberElement) => bigNumberElement.Text = "1 × 10⁻¹ = 0.1";
        // "...this. Multiplying by a negative power is the same as dividing by a positive power. Scientific
        // notation is the tool of mathematicians and scientists for dealing with unwieldy numbers,
        // and it works quite well!"
    }
}
