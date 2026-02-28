// See https://aka.ms/new-console-template for more information
using Celarix.Starfall.Layout.Helium;
using Celarix.Starfall.Layout.Helium.Transitions;
using Celarix.Starfall.Presentation;
using Celarix.Starfall.Rendering.Targets;
using OpenTK.Graphics.ES11;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;

namespace Celarix.Starfall.Playground
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var engineOptions = new PresentationEngineOptions
            {
                ErrorLevel = ErrorLevel.Display
            };
            var layoutEngine = new HeliumLayoutEngine(1280, 720);
            var presentationEngine = new PresentationEngine<HeliumScene, IHeliumTransition>(engineOptions, layoutEngine);
            var tkTarget = new SkiaTkTarget(1280, 720, 60, "Starfall Playground", presentationEngine);

            layoutEngine.SetRenderTarget(tkTarget);

            // window.Run();
        }

        private static void OldRender(GameWindow window, GRContext grContext, FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            var frameBuffer = GL.GetInteger(GetPName.DrawFramebufferBinding);
            var backendRenderTarget = new GRBackendRenderTarget(
                window.Size.X,
                window.Size.Y,
                0,
                8,
                new GRGlFramebufferInfo((uint)frameBuffer, SKColorType.Rgba8888.ToGlSizedFormat())
            );
            using var surface = SKSurface.Create(grContext, backendRenderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.CornflowerBlue);

            using var paint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            canvas.DrawCircle(window.Size.X / 2, window.Size.Y / 2, 100, paint);
            surface.Flush();
            window.SwapBuffers();
        }
    }
}