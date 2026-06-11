using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Layout.Helium.Elements;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests.Operations
{
    internal sealed class ByteOperationSlide : AtriaSlide
    {
        private const int _framesPerChange = 4;
        private const int _startAndEndHold = 60;

        private Func<byte, byte, byte> _transform;
        private int _frameNumber;
        private readonly string _operationText;
        private readonly ByteGrid _byteGridElement;
        private readonly TextBlock _yTextElement;

        public bool IsUnary => !_operationText.Contains('y', StringComparison.InvariantCultureIgnoreCase);

        public bool Completed
        {
            get
            {
                var totalFramesForAnimation = IsUnary
                    ? (_startAndEndHold * 2)
                    : 256 * _framesPerChange + (_startAndEndHold * 2);
                return _frameNumber >= totalFramesForAnimation;
            }
        }

        public ByteOperationSlide(Func<byte, byte, byte> transform,
            string operationText,
            int width,
            int height,
            double sizeMultiplier = 1d) : base(width, height)
        {
            _transform = transform;
            _operationText = operationText;
            BackgroundColor = new SColor(8, 0, 130, 255);
            var byteGridElement = new ByteGrid(b => transform(b, 0))
            {
                Size = new SSizeF(500 * (float)sizeMultiplier, 500 * (float)sizeMultiplier)
            };
            var leftRightMargin = 0.6f;
            var hCenterBasisLeft = new BasisLine(LeftCenter, RightCenter)
                .SplitAndTakeLeft(leftRightMargin);
            var gridAnchor = new BasisPoint(hCenterBasisLeft.Center, "#gridAnchor");
            byteGridElement.AnchorCenterTo(gridAnchor);
            Add([byteGridElement, gridAnchor]);
            _byteGridElement = byteGridElement;

            var textHeight = 75f * (float)sizeMultiplier;
            var operationTextElement = new TextBlock("#operation")
            {
                Text = operationText,
                Size = new SSizeF(200f * (float)sizeMultiplier, textHeight),
                Color = SColor.White,
                FontFamily = "Consolas",
                FontSize = 36f * (float)sizeMultiplier
            };
            var yTextElement = new TextBlock("#yText")
            {
                Text = "",
                Size = new SSizeF(200f * (float)sizeMultiplier, textHeight),
                Color = SColor.White,
                FontFamily = "Consolas",
                FontSize = 36f * (float)sizeMultiplier
            };
            _yTextElement = yTextElement;

            var hCenterBasisRight = new BasisLine(LeftCenter, RightCenter)
                .SplitAndTakeRight(1 - leftRightMargin);
            var operationTextAnchor = new BasisPoint(hCenterBasisRight.Center
                .Up(textHeight / 2), "#operationTextAnchor");
            var yTextAnchor = new BasisPoint(hCenterBasisRight.Center
                .Down(textHeight / 2), "#yTextAnchor");
            operationTextElement.AnchorCenterTo(operationTextAnchor);
            yTextElement.AnchorCenterTo(yTextAnchor);
            Add([operationTextElement, yTextElement, operationTextAnchor, yTextAnchor]);
        }

        public override void Update(double deltaTime)
        {
            _frameNumber += 1;
            
            // Wait 1 second before animating
            var adjustedFrameNumber = Math.Max(_frameNumber - 60, 0);

            // Change the value every _framesPerChange frames
            adjustedFrameNumber /= _framesPerChange;

            var y = adjustedFrameNumber > 255 ? 255 : (byte)adjustedFrameNumber;
            _byteGridElement.Transform = b => _transform(b, (byte)y);

            if (_operationText.Contains('y', StringComparison.InvariantCultureIgnoreCase))
            {
                _yTextElement.Text = $"y = 0x{y:X2}";
            }
        }

        public override void Initialize()
        {
            // TODO: We need to build querying so we don't have to elevate these to fields
            //operationTextElement.FitFontSize(MeasurementService);
            //yTextElement.FitFontSize(MeasurementService);
        }
    }
}
