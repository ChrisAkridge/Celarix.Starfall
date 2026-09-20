using Celarix.Starfall.Layout.Atria.Animation;
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
        private readonly AnimationContextRegistry _animationContextRegistry = new();

        public event EventHandler<Exception>? OnException;

        private FrameTime _currentFrame;

        private AtriaSlide? CurrentSlide => _currentSlideName != null && _slides.TryGetValue(_currentSlideName, out var slide) ? slide : null;

        public AtriaRuntime? Runtime { get; private set; }
        public AnimationContextRegistry AnimationContexts => _animationContextRegistry;
        public string? CurrentSlideName => _currentSlideName;
        public FrameTime CurrentFrame => _currentFrame;

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
            if (Runtime == null)
            {
                throw new InvalidOperationException("A render target must be attached before adding slides.");
            }
            if (!ReferenceEquals(slide.Runtime, Runtime))
            {
                throw new InvalidOperationException("The slide was constructed for a different Atria runtime.");
            }
            try
            {
                slide.Initialize();
            }
            catch
            {
                slide.Dispose();
                throw;
            }
            // TODO: check for duplicate names and throw if one is found
            _slides.Add(name, slide);
        }

        public void RemoveSlide(string name)
        {
            if (!_slides.ContainsKey(name))
            {
                throw new ArgumentException($"No slide with the name '{name}' exists in this layout engine.", nameof(name));
            }
            var slide = _slides[name];
            _slides.Remove(name);
            slide.Dispose();

            if (_currentSlideName?.Equals(name, StringComparison.OrdinalIgnoreCase) == true)
            {
                _currentSlideName = null;
            }
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

        public void KeyDown(SKeyboardEvent keyboardEvent)
        {
            CurrentSlide?.KeyDown(keyboardEvent);
        }

        public void KeyUp(SKeyboardEvent keyboardEvent)
        {
            CurrentSlide?.KeyUp(keyboardEvent);
        }

        public void Update(AtriaSlide slide, FrameTime frameTime)
        {
            _currentFrame = frameTime;
            _animationContextRegistry.UpdateAll(frameTime);
            slide.Update(frameTime);
        }

        public void Render(AtriaSlide slide)
        {
            ThrowIfNoRenderTarget();
            slide.Render(_renderTarget!);
            _renderTarget!.Complete();
        }

        public void Attach(IRenderTarget renderTarget)
        {
            if (_renderTarget != null)
            {
                throw new InvalidOperationException("Render target has already been set for this layout engine. It cannot be changed after being set.");
            }

            _renderTarget = renderTarget;
            Runtime = new AtriaRuntime(new MeasurementService(renderTarget), _debugMode,
                _animationContextRegistry, new SSizeF(viewportWidth, viewportHeight));
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

            try
            {
                var delta = TimeSpan.FromSeconds(deltaTime);
                var frameTime = new FrameTime(_currentFrame.Number + 1,
                    _currentFrame.Elapsed + delta,
                    delta);
                Update(CurrentSlide, frameTime);
                Render(CurrentSlide);
            }
            catch (Exception ex)
            {
                if (OnException != null)
                {
                    OnException.Invoke(this, ex);
                }
                else
                {
                    Console.WriteLine($"Unhandled exception in AtriaLayoutEngine.OnFrameRequested: {ex}");
                }
            }
        }
    }
}
