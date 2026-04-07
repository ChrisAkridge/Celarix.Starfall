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
            if (surface?.Canvas == null) { return; }

            SkiaCommon.Clear(surface.Canvas, color);
        }

        public void DrawRectangle(SRectF bounds, SColor color, SPaintStyle paintStyle, SAngle rotation)
        {
            // TODO: Implement rotation
            if (surface?.Canvas == null) { return; }
            SkiaCommon.DrawRectangle(surface.Canvas, bounds, color, paintStyle);
        }

        public void DrawText(string text, SFont font, SRectF bounds, SColor color, SAngle rotation, Alignment alignment = Alignment.Center)
        {
            // TODO: Implement rotation
            if (surface?.Canvas == null) { return; }
            SkiaCommon.DrawText(surface.Canvas, text, font, bounds, color, rotation, alignment);
        }

        public void DrawTextDirectly(string text, SFont font, SRectF bounds, SColor color, SAngle rotation)
        {
            // TODO: Implement rotation
            if (surface?.Canvas == null) { return; }
            SkiaCommon.DrawTextDirectly(surface.Canvas, text, font, bounds, color, rotation);
        }

        public void DrawLine(SPointF start, SPointF end, SColor color, float thickness)
        {
            if (surface?.Canvas == null) { return; }
            SkiaCommon.DrawLine(surface.Canvas, start, end, color, thickness);
        }

        public void DrawImageFromFile(string filePath, SRectF bounds, double opacity, SAngle rotation)
        {
            if (surface?.Canvas == null) { return; }
            SkiaCommon.DrawImageFromFile(surface.Canvas, filePath, bounds, opacity, rotation);
        }

        // TODO: Cache these text/font measurements, as a lot of the time, the same text will be measured
        // repeatedly, especially on animated scenes.
        public float FitTextToWidth(string text, SFont font, float width) => SkiaTextRendering.FitTextToWidth(text, font, width);

        public float FitTextToHeight(string text, SFont font, float height) => SkiaTextRendering.FitTextToHeight(text, font, height);

        public SSizeF MeasureText(string text, SFont font) => SkiaTextRendering.GetFont(font).MeasureShapedText(text);
    }
}
