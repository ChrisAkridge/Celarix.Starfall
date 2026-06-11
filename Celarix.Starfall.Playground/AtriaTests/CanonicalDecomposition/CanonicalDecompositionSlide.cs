using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.AtriaTests.CanonicalDecomposition
{
    internal sealed class CanonicalDecompositionSlide : AtriaSlide
    {
        // I know I'm supposed to be using AtriaElements here
        // but it's late and I want to write code and not be lost in the weeds of overthinking

        private SPointF _cameraCenter;
        private double _cameraZoom;    // 1.0 = pixels are pixels, 2.0 = pixels are 2x2 blocks of pixels, etc.
        private SPointF _cameraCenterTarget;
        private double _cameraZoomTarget;

        private double _elapsedTime;
        private double? _currentCameraMoveDuration;
        private double? _currentCameraMoveStart;
        private double? _currentCameraMoveEnd;

        // We really need some kind of better keyframing/phase system in Starfall.
        // Something like the non-linear video editors that you can drag and drop
        // timeline blocks on.
        private int _currentPhase = 0;

        private readonly SImage _image;
        private readonly SRectF _imageRect;

        private SRectF CameraViewport
        {
            get
            {
                var zoomScalingFactor = Math.Pow(2, _cameraZoom);
                var viewportLeft = _cameraCenter.X - ((Size.Width / zoomScalingFactor) / 2);
                var viewportTop = _cameraCenter.Y - ((Size.Height / zoomScalingFactor) / 2);
                return new SRectF(viewportLeft, viewportTop, Size.Width / zoomScalingFactor, Size.Height / zoomScalingFactor);
            }
        }

        public CanonicalDecompositionSlide(string imagePath, int width, int height) : base(width, height)
        {
            BackgroundColor = new SColor(8, 0, 130, 255);

            _cameraCenter = Center;
            _cameraZoom = 0d;
            _cameraCenterTarget = _cameraCenter;
            _cameraZoomTarget = _cameraZoom;

            _image = SImage.FromSixLaborsImage(Image.Load<Rgba32>(imagePath));
            _imageRect = new SSizeF(_image.Width, _image.Height).CenterAt(Center);
        }

        public override void Render(IRenderTarget target)
        {
            base.Render(target);

            var viewport = CameraViewport;
            var zoomScalingFactor = Math.Pow(2, _cameraZoom);

            var zoomedImagePosition = (_imageRect.Position - viewport.Position) * zoomScalingFactor;
            var zoomedImageSize = _imageRect.Size * zoomScalingFactor;
            var zoomedImageRect = zoomedImagePosition.WithSize(zoomedImageSize);

            if (_cameraZoom <= 6)
            {
                target.DrawImage(_image, zoomedImageRect);
            }
        }

        public override void Update(double deltaTime)
        {
            _elapsedTime += deltaTime;

            if (_currentCameraMoveEnd.HasValue)
            {
                var moveDuration = _currentCameraMoveEnd.Value - _currentCameraMoveStart!.Value;
                var moveLinearProgress = (_elapsedTime - _currentCameraMoveStart.Value) / moveDuration;
                if (moveLinearProgress >= 1)
                {
                    _cameraCenter = _cameraCenterTarget;
                    _cameraZoom = _cameraZoomTarget;
                    _currentCameraMoveEnd = null;
                    _currentCameraMoveDuration = null;
                }
                else
                {
                    var t = moveLinearProgress;
                    _cameraCenter = MathHelpers.Ease(_cameraCenter, _cameraCenterTarget, t, Easings.Smoothstep);
                    _cameraZoom = MathHelpers.Ease(_cameraZoom, _cameraZoomTarget, t, Easings.Smoothstep);
                }
            }
            else
            {
                if (_currentPhase == 0)
                {
                    // Move to top-left
                    _currentCameraMoveDuration = 10d;
                    _currentCameraMoveStart = _elapsedTime;
                    _currentCameraMoveEnd = _currentCameraMoveStart + _currentCameraMoveDuration;
                    _cameraCenterTarget = _imageRect.TopLeft;
                    _cameraZoomTarget = 0d; // no zoom yet
                    _currentPhase = 1;
                }
                else if (_currentPhase == 1)
                {
                    // Hard zoom on top-left
                    _currentCameraMoveDuration = 20d;
                    _currentCameraMoveStart = _elapsedTime;
                    _currentCameraMoveEnd = _currentCameraMoveStart + _currentCameraMoveDuration;
                    _cameraCenterTarget = _imageRect.TopLeft;
                    _cameraZoomTarget = 6d;
                    _currentPhase = 2;
                }
                // Phases beyond the last implemented one do nothing on purpose
            }
        }

        public override void Initialize()
        {

        }

        //private List<(SRectF Rect, SColor Color)> GetCanonicalDecompositionRects(Rectangle imageRect)
        //{
        //    var result = new List<(SRectF Rect, SColor Color)>();

        //    for (int y = 0; y < imageRect.Height; y++)
        //    {
        //        for (int x = 0; x < imageRect.Width; x++)
        //        {
        //            var pixelColor = _image.GetPixel(x, y);
        //            var rect = new SRectF(x, y, 1, 1);
        //            result.Add((rect, pixelColor));
        //        }
        //    }
        //}
    }
}
