using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;

namespace Celarix.Starfall.Harness;

/// <summary>
/// The audience-facing window: full screen on the chosen display, showing the rendered frames.
/// Right / Page Down / Space advance, Left / Page Up rewind, Escape ends the presentation.
/// </summary>
public partial class PresentationWindow : Window
{
    private readonly PresentationController? _controller;
    private bool _running;

    // The XAML loader needs a parameterless constructor.
    public PresentationWindow()
    {
        InitializeComponent();
    }

    internal PresentationWindow(PresentationController controller, Screen screen) : this()
    {
        _controller = controller;
        FrameImage.Source = controller.Framebuffer.Bitmap;
        controller.Target.FrameCompleted += (_, _) => FrameImage.InvalidateVisual();

        WindowStartupLocation = WindowStartupLocation.Manual;
        Position = screen.Bounds.Position;
        Width = screen.Bounds.Width / screen.Scaling;
        Height = screen.Bounds.Height / screen.Scaling;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        WindowState = WindowState.FullScreen;
        _running = true;
        RequestAnimationFrame(OnAnimationFrame);
    }

    protected override void OnClosed(EventArgs e)
    {
        _running = false;
        base.OnClosed(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (_controller == null)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Right:
            case Key.PageDown:
            case Key.Space:
                _controller.Advance();
                e.Handled = true;
                break;
            case Key.Left:
            case Key.PageUp:
                _controller.Rewind();
                e.Handled = true;
                break;
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
        }
    }

    private void OnAnimationFrame(TimeSpan timestamp)
    {
        if (!_running || _controller == null)
        {
            return;
        }

        _controller.RenderFrame(timestamp);
        RequestAnimationFrame(OnAnimationFrame);
    }
}
