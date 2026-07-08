using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Rendering.Initialization;
using Celarix.Starfall.Rendering.Models;
using FastCache;
using OpenTK.Graphics.ES11;
using OpenTK.Windowing.Common;
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
        private readonly SMonitorInfo[] _monitorInfos;
        private GRContext? grContext;
        private SKSurface? surface;

        public bool CanAnimate => true;
        public bool IsAnimating
        {
            get => !window.IsEventDriven;
            set => window.IsEventDriven = !value;
        }

        public IReadOnlyList<SMonitorInfo> MonitorInfos => _monitorInfos;

        public SkiaTkTarget(int width,
            int height,
            int desiredFrameRate,
            string title,
            INotifyFrameRequested frameRequested,
            int? monitorIndex = null)
        {
            _monitorInfos = [.. MonitorInfoProvider.GetMonitorInfos()];

            this.frameRequested = frameRequested;
            var nativeSettings = new NativeWindowSettings()
            {
                ClientSize = new OpenTK.Mathematics.Vector2i(width, height),
                Title = title,
            };

            if (monitorIndex != null)
            {
                nativeSettings.CurrentMonitor = new MonitorHandle(_monitorInfos[monitorIndex.Value].Handle);
                nativeSettings.WindowState = WindowState.Normal;
                nativeSettings.WindowBorder = WindowBorder.Hidden;
                nativeSettings.StartVisible = true;
                nativeSettings.StartFocused = false;
                var monitor = _monitorInfos[monitorIndex.Value];
                nativeSettings.Location = new OpenTK.Mathematics.Vector2i((int)monitor.WorkArea.X, (int)monitor.WorkArea.Y);

                // Attempt to fix black flickering on focus change (https://github.com/opentk/opentk/issues/1857)
                // Kind of an odd hack, but...
                nativeSettings.ClientSize = new OpenTK.Mathematics.Vector2i(monitor.Width - 1, monitor.Height - 1);
                // ...maybe it works.
            }

            var windowSettings = new GameWindowSettings
            {
                UpdateFrequency = desiredFrameRate
            };
            window = new GameWindow(windowSettings, nativeSettings)
            {
                VSync = VSyncMode.On
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

        private void Window_RenderFrame(object? sender, FrameEventArgs e)
        {
            frameRequested.OnFrameRequested(e.Time);
        }

        public void Start()
        {
            window.Run();
        }

        // ======
        // Multi-monitor and Fullscreen API
        // ======
        public void SetFullscreen(int monitorIndex)
        {
            if (monitorIndex < 0 || monitorIndex >= _monitorInfos.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(monitorIndex), $"Monitor index must be between 0 and {_monitorInfos.Length - 1}.");
            }

            var monitor = _monitorInfos[monitorIndex];
            window.MakeFullscreen(new MonitorHandle(monitor.Handle));
            window.WindowBorder = WindowBorder.Hidden;
        }

        public void ClearFullscreen()
        {
            window.WindowBorder = WindowBorder.Resizable;
            window.WindowState = WindowState.Normal;
        }

        // ======
        // Inbound Event Handlers
        // ======
        public event EventHandler<KeyboardKeyEventArgs> KeyUp;

        private void RegisterEventHandlers()
        {
            window.KeyUp += OnKeyUp;
        }

        private void OnKeyUp(KeyboardKeyEventArgs e)
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
            SkiaCommon.DrawRectangle(surface.Canvas, bounds, color, paintStyle, rotation);
        }

        public void DrawEllipse(SPointF center, SSizeF size, SColor color, SPaintStyle paintStyle)
        {
            if (surface?.Canvas == null) { return; }
            SkiaCommon.DrawEllipse(surface.Canvas, center, size, color, paintStyle);
        }

        public void DrawRadialGradientCircle(SPointF center, double radius, SColor[] colors, double[] colorPositions, SShaderTileMode tileMode, SBlendMode blendMode)
        {
            if (surface?.Canvas == null) { return; }
            SkiaCommon.DrawRadialGradientCircle(surface.Canvas, center, radius, colors, colorPositions, tileMode, blendMode);
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

        public void DrawImage(SImage image, SRectF bounds, double opacity = 1d, SAngle? rotation = null)
        {
            if (surface?.Canvas == null) { return; }
            SkiaCommon.DrawImage(surface.Canvas, image, bounds, opacity, rotation ?? SAngle.Zero);
        }

        public void DrawCroppedImage(SImage image, SRectF sourceRect, SRectF destRect, double opacity = 1d)
        {
            if (surface?.Canvas == null) { return; }
            SkiaCommon.DrawCroppedImage(surface.Canvas, image, sourceRect, destRect, opacity);
        }

        public void DrawPoint(SPointF point, SColor color)
        {
            if (surface?.Canvas == null) { return; }
            SkiaCommon.DrawPoint(surface.Canvas, point, color);
        }

        public IOffscreenRenderTarget CreateOffscreenTarget(SSizeF size) => new SkiaOffscreenTarget((int)size.Width, (int)size.Height, grContext);

        // TODO: Cache these text/font measurements, as a lot of the time, the same text will be measured
        // repeatedly, especially on animated scenes.
        public float FitTextToWidth(string text, SFont font, float width) => SkiaTextRendering.FitTextToWidth(text, font, width);

        public float FitTextToHeight(string text, SFont font, float height) => SkiaTextRendering.FitTextToHeight(text, font, height);

        public SSizeF MeasureText(string text, SFont font) => SkiaTextRendering.GetFont(font).MeasureShapedText(text);
    }
}
