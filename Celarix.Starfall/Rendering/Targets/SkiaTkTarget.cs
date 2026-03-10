using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Rendering.Models;
using FastCache;
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

            SkiaTextRendering.SetShaperCacheDuration(30000);
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

        public void DrawRectangle(SRectF bounds, SColor color, SPaintStyle paintStyle, SAngle rotation)
        {
            // TODO: Implement rotation
            if (surface == null) { return; }
            using var paint = new SKPaint
            {
                Color = color.ToSKColor(),
                IsAntialias = true,
                Style = paintStyle.ToSKPaintStyle()
            };
            surface.Canvas.DrawRect(bounds.ToSKRect(), paint);
        }

        public void DrawText(string text, SFont font, SRectF bounds, SColor color, SAngle rotation, Alignment alignment = Alignment.Center)
        {
            // TODO: Implement rotation
            if (surface == null) { return; }

            SKFont skFont;
            var useFontSize = font.Size != null;
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

            skFont = useFontSize
                ? GetFont(font)
                : GetFont(font.WithSize(fittedSize));

            SSizeF measuredNewSize = skFont.MeasureShapedText(text);
            if (!useFontSize)
            {
                bounds = AlignmentHelper.Align(alignment, bounds, measuredNewSize).WithSize(measuredNewSize);
            }
            else if (measuredNewSize.Width > bounds.Width || measuredNewSize.Height > bounds.Height)
            {
                // Too big! Reduce the font size until it fits. We can do this by finding the longest
                // axis of the measured size and computing the scale factor between that and the corresponding
                // axis of the bounds, then applying that scale factor to the font size.
                var widthScale = (float)bounds.Width / measuredNewSize.Width;
                var heightScale = (float)bounds.Height / measuredNewSize.Height;
                var scale = Math.Min(widthScale, heightScale);
                skFont = font.WithSize(skFont.Size * (float)scale).ToSKFont();
            }

            surface.Canvas.DrawShapedText(text,
                (float)bounds.Left,
                (float)bounds.Top - skFont.Metrics.Ascent,
                skFont,
                paint);
        }

        public void DrawTextDirectly(string text, SFont font, SRectF bounds, SColor color, SAngle rotation)
        {
            // TODO: Implement rotation
            if (surface == null) { return; }

            using var paint = new SKPaint
            {
                Color = color.ToSKColor(),
                IsAntialias = true
            };

            // Use the font size provided by the layout system as-is
            var skFont = GetFont(font);

            // Measure for alignment only; DO NOT rescale the font
            var measured = skFont.MeasureShapedText(text);

            var drawLeft = (float)bounds.Left;
            var drawTop = (float)bounds.Top;

            // Convert top-based coordinate to baseline for Skia
            var baselineY = drawTop - skFont.Metrics.Ascent;

            surface.Canvas.DrawShapedText(text, drawLeft, baselineY, skFont, paint);
        }

        // TODO: Cache these text/font measurements, as a lot of the time, the same text will be measured
        // repeatedly, especially on animated scenes.
        public float FitTextToWidth(string text, SFont font, float width)
        {
            var skFont = font.ToSKFont();
            using var paint = new SKPaint
            {
                IsAntialias = true
            };
            var measuredWidth = skFont.MeasureShapedText(text).Width;
            var scale = width / measuredWidth;
            return (font.Size ?? 12f) * (float)scale;
        }

        public float FitTextToHeight(string text, SFont font, float height)
        {
            var skFont = GetFont(font);
            using var paint = new SKPaint
            {
                IsAntialias = true
            };
            var metrics = skFont.Metrics;
            var measuredHeight = metrics.Descent - metrics.Ascent;
            var scale = height / measuredHeight;
            return (font.Size ?? 12f) * scale;
        }

        public SSizeF MeasureText(string text, SFont font) => GetFont(font).MeasureShapedText(text);

        #region Text Caching
        private static readonly double DefaultCacheDurationMinutes = 60d;

        private static SKFont GetFont(SFont font)
        {
            var cacheKey = font.ToCacheKey();
            if (Cached<SKFont>.TryGet(cacheKey, out var cachedFont))
            {
                return cachedFont;
            }
            else
            {
                var skFont = font.ToSKFont();
                return cachedFont.Save(skFont, TimeSpan.FromMinutes(DefaultCacheDurationMinutes));
            }
        }
        #endregion
    }
}
