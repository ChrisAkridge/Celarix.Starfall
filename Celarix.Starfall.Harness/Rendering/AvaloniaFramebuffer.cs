using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Celarix.Starfall.Rendering.Targets;
using SkiaSharp;

namespace Celarix.Starfall.Harness.Rendering;

/// <summary>
/// An Avalonia <see cref="WriteableBitmap"/> that a <see cref="SkiaCanvasTarget"/> draws into.
/// Each frame locks the bitmap, wraps its pixels in a Skia surface, and unlocks it afterwards.
/// </summary>
internal sealed class AvaloniaFramebuffer : IDisposable
{
    private readonly SKImageInfo _info;
    private ILockedFramebuffer? _locked;
    private SKSurface? _surface;

    public AvaloniaFramebuffer(int width, int height)
    {
        _info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        Bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
    }

    public WriteableBitmap Bitmap { get; }

    public void BeginFrame(SkiaCanvasTarget target)
    {
        EndFrame(target);
        _locked = Bitmap.Lock();
        _surface = SKSurface.Create(_info, _locked.Address, _locked.RowBytes)
            ?? throw new InvalidOperationException("Could not create a Skia surface over the Avalonia bitmap.");
        target.BeginFrame(_surface.Canvas);
    }

    public void EndFrame(SkiaCanvasTarget target)
    {
        target.EndFrame();
        _surface?.Dispose();
        _surface = null;
        _locked?.Dispose();
        _locked = null;
    }

    public void Dispose()
    {
        _surface?.Dispose();
        _locked?.Dispose();
        Bitmap.Dispose();
    }
}
