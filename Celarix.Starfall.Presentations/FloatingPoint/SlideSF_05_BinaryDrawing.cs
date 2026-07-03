using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Presentations.FloatingPoint.Elements.BinaryDrawing;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Presentations.FloatingPoint
{
    internal sealed class SlideSF_05_BinaryDrawing : AtriaSlide
    {
        // States:
        // 0. Nothing shown on screen
        // 1. Bytes slide in from below (ShowBytes)
        // 2. Boxes are shown (ShowBoxes)
        // 3. Boxes are colored (ColorBoxes)
        // 4. Boxes are merged (MergeBoxes)
        // 5. Pixel row is built (BuildPixelRow)
        // 6. Image is filled (FillImage)
        // 7. Console asks if they want another file to be loaded; if yes, go to 8, if no, go to 9
        // 8. Open an OpenFileDialog to select a file, then go to 0
        // 9. Final state, can advance
        private int _state;
        private bool _elementsCreated;
        private string _currentFilePath;
        private long _currentFileSize;
        private SSize? _currentBinaryDrawnImageSize;

        private string InfoText
        {
            get
            {
                if (_currentBinaryDrawnImageSize == null)
                {
                    return $"{Path.GetFileName(_currentFilePath)} ({_currentFileSize:#,###} bytes)";
                }
                SSize size = _currentBinaryDrawnImageSize.Value;
                return $"{Path.GetFileName(_currentFilePath)} ({_currentFileSize:#,###} bytes) - {size.Width}x{size.Height}";
            }
        }

        public SlideSF_05_BinaryDrawing(int width, int height) : base(width, height)
        {
            _currentFilePath = @"Assets\Files\Celarix.Starfall.dll";
            _currentFileSize = new FileInfo(_currentFilePath).Length;
        }

        public override void Initialize()
        {
            BackgroundColor = Constants.StarfallBackground;
        }

        public override SlideAdvanceResult Advance()
        {
            switch (_state)
            {
                case 0:
                    return LoadNextFile();
                case 1:
                    return ShowBytes();
                case 2:
                    return ShowBoxes();
                case 3:
                    return ColorBoxes();
                case 4:
                    return MergeBoxes();
                case 5:
                    return BuildPixelRow();
                case 6:
                    return FillImage();
                case 7:
                    return CheckForNextImage();
                default:
                    throw new InvalidOperationException($"Unexpected state {_state} in {nameof(SlideSF_05_BinaryDrawing)}.");
            }
        }

        private SlideAdvanceResult LoadNextFile()
        {
            if (!_elementsCreated)
            {
                var createdBinaryDrawingElement = new BinaryDrawingExampleElement("#binaryDrawingExample")
                {
                    Position = SPointF.Zero,
                    Size = Size,
                };
                createdBinaryDrawingElement.SetFontSize(36f, MeasurementService);

                var infoText = new TextBlock("#infoText")
                {
                    Text = InfoText,
                    FontFamily = "Consolas",
                    FontSize = 24,
                    Color = SColor.White
                };
                var infoTextAnchor = new BasisPoint(TopCenter.Down(10d), "#infoTextAnchor");
                infoText.AnchorTopCenterTo(infoTextAnchor);

                Add([createdBinaryDrawingElement, infoText, infoTextAnchor])
                    .AnimateBasic(0.5d, AnimationTypes.FadeIn, Easings.Linear);
            }

            var binaryDrawingElement = (BinaryDrawingExampleElement)Query("#binaryDrawingExample").Single();
            binaryDrawingElement.SetDataFromFile(_currentFilePath);
            _state = 1;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult ShowBytes()
        {
            var binaryDrawingElement = (BinaryDrawingExampleElement)Query("#binaryDrawingExample").Single();
            binaryDrawingElement.ShowBytes();
            _state = 2;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult ShowBoxes()
        {
            var binaryDrawingElement = (BinaryDrawingExampleElement)Query("#binaryDrawingExample").Single();
            binaryDrawingElement.ShowBoxes();
            _state = 3;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult ColorBoxes()
        {
            var binaryDrawingElement = (BinaryDrawingExampleElement)Query("#binaryDrawingExample").Single();
            binaryDrawingElement.ColorBoxes();
            _state = 4;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult MergeBoxes()
        {
            var binaryDrawingElement = (BinaryDrawingExampleElement)Query("#binaryDrawingExample").Single();
            binaryDrawingElement.MergeBoxes();
            _state = 5;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult BuildPixelRow()
        {
            var binaryDrawingElement = (BinaryDrawingExampleElement)Query("#binaryDrawingExample").Single();
            binaryDrawingElement.BuildPixelRow();
            _state = 6;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult FillImage()
        {
            var binaryDrawingElement = (BinaryDrawingExampleElement)Query("#binaryDrawingExample").Single();
            binaryDrawingElement.FillImage();

            var width = (int)Math.Ceiling(Math.Sqrt(_currentFileSize));
            var height = (int)Math.Ceiling((double)_currentFileSize / width);
            _currentBinaryDrawnImageSize = new(width, height);
            UpdateInfoText();
            _state = 7;
            return SlideAdvanceResult.InternalStateChanged;
        }

        private SlideAdvanceResult CheckForNextImage()
        {
            Console.Write("Would you like to load another file? (y/n): ");
            var response = Console.ReadLine();
            if (response?.Trim().ToLower() == "y")
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    _currentFilePath = openFileDialog.FileName;
                    _currentFileSize = new FileInfo(_currentFilePath).Length;
                    _currentBinaryDrawnImageSize = null;
                    UpdateInfoText();
                    _state = 0;
                    return SlideAdvanceResult.InternalStateChanged;
                }
                else
                {
                    // User canceled the file dialog, stay in the current state
                    return SlideAdvanceResult.InternalStateChanged;
                }
            }
            else
            {
                // User does not want to load another file, can advance to next slide
                _state = 8;
                return SlideAdvanceResult.InternalStateChanged;
            }
        }

        private void UpdateInfoText()
        {
            var infoText = (TextBlock)Query("#infoText").Single();
            infoText.Text = InfoText;
        }
    }
}
