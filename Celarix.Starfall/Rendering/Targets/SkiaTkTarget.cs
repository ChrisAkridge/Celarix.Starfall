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
        }

        private void Window_RenderFrame(object? sender, OpenTK.Windowing.Common.FrameEventArgs e)
        {
            frameRequested.OnFrameRequested(e.Time);
        }

        public void Start()
        {
            window.Run();
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
            if (surface == null) { return; }
            using var paint = new SKPaint
            {
                Color = color.ToSKColor(),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            surface.Canvas.DrawRect(bounds.ToSKRect(), paint);
        }
    }
}
