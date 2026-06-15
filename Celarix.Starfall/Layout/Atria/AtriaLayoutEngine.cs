using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria
{
    public class AtriaLayoutEngine : INotifyFrameRequested
    {
        private IRenderTarget? _renderTarget;
        private int viewportWidth;
        private int viewportHeight;
        private Dictionary<string, AtriaSlide> _slides = new();
        private string? _currentSlideName;
        private DebugMode _debugMode;

        public static int GlobalFrameNumber { get; internal set; }

        private AtriaSlide? CurrentSlide => _currentSlideName != null && _slides.TryGetValue(_currentSlideName, out var slide) ? slide : null;
        
        public MeasurementService? MeasurementService { get; set; }

        public AtriaLayoutEngine(int viewportWidth, int viewportHeight)
        {
            _debugMode = new DebugMode();
            this.viewportWidth = viewportWidth;
            this.viewportHeight = viewportHeight;
        }

        public void Start()
        {
            ThrowIfNoRenderTarget();
            _renderTarget!.Start();
        }

        public void AddSlide(AtriaSlide slide, string name)
        {
            slide.SetProtectedProperties(MeasurementService ?? throw new InvalidOperationException("MeasurementService must be set on the layout engine before adding slides."),
                _debugMode);
            slide.Initialize();
            // TODO: check for duplicate names and throw if one is found
            _slides.Add(name, slide);
        }

        public void SetCurrentSlide(string name)
        {
            if (!_slides.ContainsKey(name))
            {
                throw new ArgumentException($"No slide with the name '{name}' exists in this layout engine.", nameof(name));
            }
            _currentSlideName = name;
        }

        // CANIMPROVE: We need a solid model for slide-to-slide transitions that aren't just
        // "show one slide than instant cut to another". But fading between slides is more than
        // just "render both and adjust opacity" because there's only one render target. I also
        // don't like "freeze the last slide, take its final frame, and fade that out" because
        // internal animations just stop. Having the render target draw two frame buffers that
        // we layer is not a bad idea but can lead to performance issues where two slides that
        // are fine on their own are way too slow together.

        public void Update(AtriaSlide slide, double deltaTime)
        {
            GlobalFrameNumber += 1;
            slide.Update(deltaTime);
        }

        public void Render(AtriaSlide slide)
        {
            ThrowIfNoRenderTarget();
            slide.Render(_renderTarget!);
            _renderTarget!.Complete();
        }

        public void SetRenderTarget(IRenderTarget renderTarget)
        {
            if (_renderTarget != null)
            {
                throw new InvalidOperationException("Render target has already been set for this layout engine. It cannot be changed after being set.");
            }

            _renderTarget = renderTarget;
        }

        public SlideAdvanceResult RewindCurrentSlide()
        {
            if (CurrentSlide == null) { return SlideAdvanceResult.InternalStateChanged; }
            return CurrentSlide.Rewind();
        }

        public SlideAdvanceResult AdvanceCurrentSlide()
        {
            if (CurrentSlide == null) { return SlideAdvanceResult.InternalStateChanged; }
            return CurrentSlide.Advance();
        }

        private void ThrowIfNoRenderTarget()
        {
            // No, seriously, always throw. If we can't render anywhere, how are we supposed to have
            // an ErrorLevel of Display?
            if (_renderTarget == null)
            {
                throw new InvalidOperationException("No render target has been set for this layout engine. Please call SetRenderTarget before attempting to render or transition.");
            }
        }

        public void OnFrameRequested(double deltaTime)
        {
            if (CurrentSlide == null)
            {
                _renderTarget?.Clear(SColor.Blue);
                return;
            }

            Update(CurrentSlide, deltaTime);
            Render(CurrentSlide);
        }
    }
}
