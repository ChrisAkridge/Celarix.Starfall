using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Rendering.Models;
using OpenTK.Graphics.ES11;
using OpenTK.Windowing.Desktop;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using static Celarix.Starfall.Rendering.Converters.SkiaConverters;

namespace Celarix.Starfall.Rendering.Targets
{
    public sealed class SkiaTkTarget : IRenderTarget
    {
        private readonly INotifyFrameRequested frameRequested;
        private readonly GameWindow window;
        private GRContext? grContext;
        private SKSurface? surface;

        public bool CanAnimate => true;
        public bool IsAnimating
        {
            get => !window.IsEventDriven;
            set => window.IsEventDriven = !value;
        }

        public SkiaTkTarget(int width, int height, int desiredFrameRate, string title, INotifyFrameRequested frameRequested)
        {
            this.frameRequested = frameRequested;
            var nativeSettings = new NativeWindowSettings()
            {
                ClientSize = new OpenTK.Mathematics.Vector2i(width, height),
                Title = title,
            };
            var windowSettings = new GameWindowSettings
            {
                UpdateFrequency = desiredFrameRate
            };
            window = new GameWindow(windowSettings, nativeSettings)
            {
                VSync = OpenTK.Windowing.Common.VSyncMode.On
            };

            window.Load += () =>
            {
                // Ensure viewport matches framebuffer
                GL.Viewport(0, 0, window.ClientSize.X, window.ClientSize.Y);

                // If OpenTK exposes FramebufferSize or similar, prefer that:
                var fbWidth = window.ClientSize.X;
                var fbHeight = window.ClientSize.Y;

                grContext = GRContext.CreateGl();
                surface = SKSurface.Create(
                    grContext,
                    new GRBackendRenderTarget(
                        fbWidth,
                        fbHeight,
                        0,
                        8,
                        new GRGlFramebufferInfo(
                            (uint)GL.GetInteger(GetPName.DrawFramebufferBinding),
                            SKColorType.Rgba8888.ToGlSizedFormat())),
                    GRSurfaceOrigin.BottomLeft,
                    SKColorType.Rgba8888);
            };
            window.RenderFrame += args => Window_RenderFrame(window, args);
            RegisterEventHandlers();
        }

        private void Window_RenderFrame(object? sender, OpenTK.Windowing.Common.FrameEventArgs e)
        {
            frameRequested.OnFrameRequested(e.Time);
        }

        public void Start()
        {
            window.Run();
        }

        // ======
        // Inbound Event Handlers
        // ======
        public event EventHandler<OpenTK.Windowing.Common.KeyboardKeyEventArgs> KeyUp;

        private void RegisterEventHandlers()
        {
            window.KeyUp += OnKeyUp;
        }

        private void OnKeyUp(OpenTK.Windowing.Common.KeyboardKeyEventArgs e)
        {
            KeyUp?.Invoke(this, e);
        }

        // =======
        // Rendering API
        //  The call chain is:
        //  OpenTK -> Window_RenderFrame -> INotifyFrameRequested.OnFrameRequested -> PresentationEngine.OnFrameRequested -> LayoutEngine.Render
        //  The below methods must only be called during the LayoutEngine.Render call.
        // =======

        public void Complete()
        {
            surface?.Flush();
            window.SwapBuffers();
        }

        public void Clear(SColor color)
        {
            surface?.Canvas.Clear(color.ToSKColor());
        }

        public void DrawRectangle(SRectF bounds, SColor color, SAngle rotation)
        {
            // TODO: Implement rotation
            if (surface == null) { return; }
            using var paint = new SKPaint
            {
                Color = color.ToSKColor(),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            surface.Canvas.DrawRect(bounds.ToSKRect(), paint);
        }

        public void DrawText(string text, SFont font, SRectF bounds, SColor color, SAngle rotation)
        {
            // TODO: Implement rotation
            if (surface == null) { return; }

            SKFont skFont;
            var skColor = color.ToSKColor();

            using var paint = new SKPaint
            {
                Color = skColor,
                IsAntialias = true
            };

            // Expand the text to fit the widest dimension it can. We then want to center the text in
            // the bounds, which could mean quite a big offset on the other dimension (i.e. if the bounds
            // were 1000x100000 or something).
            float fittedSize;
            if (bounds.Height >= bounds.Width)
            {
                // ###
                // ###
                // ###
                // ###
                // ###, so the text can't be very wide
                fittedSize = FitTextToWidth(text, font, (float)bounds.Width);
            }
            else
            {
                // #####
                // #####, so the text can't be very tall
                fittedSize = FitTextToHeight(text, font, (float)bounds.Height);
            }

            skFont = font.WithSize(fittedSize).ToSKFont();
            SSizeF measuredNewSize = new(skFont.MeasureText(text, paint), skFont.Metrics.Descent - skFont.Metrics.Ascent);
            bounds = AlignmentHelper.Align(Alignment.Center, bounds, measuredNewSize).WithSize(measuredNewSize);

            surface.Canvas.DrawText(text,
                (float)bounds.Left,
                (float)bounds.Top - skFont.Metrics.Ascent,
                skFont,
                paint);
        }

        public float FitTextToWidth(string text, SFont font, float width)
        {
            var skFont = font.ToSKFont();
            using var paint = new SKPaint
            {
                IsAntialias = true
            };
            var measuredWidth = skFont.MeasureText(text, paint);
            var scale = width / measuredWidth;
            return font.Size * scale;
        }

        public float FitTextToHeight(string text, SFont font, float height)
        {
            var skFont = font.ToSKFont();
            using var paint = new SKPaint
            {
                IsAntialias = true
            };
            var metrics = skFont.Metrics;
            var measuredHeight = metrics.Descent - metrics.Ascent;
            var scale = height / measuredHeight;
            return font.Size * scale;
        }

        public SSizeF MeasureText(string text, SFont font)
        {
            var skFont = font.ToSKFont();
            using var paint = new SKPaint
            {
                IsAntialias = true
            };
            var width = skFont.MeasureText(text, paint);
            var metrics = skFont.Metrics;
            var height = metrics.Descent - metrics.Ascent;
            return new SSizeF(width, height);
        }
    }
}
