using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Celarix.Starfall.Atria;

namespace Celarix.Starfall.Harness;

/// <summary>
/// The presenter's control window: pick a presentation, display and size, start it, then follow
/// and steer the slides, beats and notes while the audience window shows the frames.
/// </summary>
public partial class OperatorWindow : Window
{
    private PresentationController? _controller;
    private PresentationWindow? _presentationWindow;

    public OperatorWindow()
    {
        InitializeComponent();

        DeckPicker.ItemsSource = DeckCatalog.All.Select(DescribeDeck).ToList();
        DeckPicker.SelectedIndex = 0;
        DeckPicker.SelectionChanged += (_, _) => ShowSlideList();
        ShowSlideList();
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        var screens = Screens.All.ToList();
        ScreenPicker.ItemsSource = screens.Select(DescribeScreen).ToList();

        // Default to a display other than the one this window is on, like a projector.
        var current = Screens.ScreenFromWindow(this);
        var other = screens.FindIndex(s => !s.Equals(current));
        ScreenPicker.SelectedIndex = other >= 0 ? other : 0;
    }

    protected override void OnClosed(EventArgs e)
    {
        StopPresentation();
        base.OnClosed(e);
    }

    private PresentationDefinition SelectedDeck => DeckCatalog.All[Math.Max(DeckPicker.SelectedIndex, 0)];

    private void StartButton_Click(object? sender, RoutedEventArgs e)
    {
        StopPresentation();

        var screens = Screens.All.ToList();
        if (screens.Count == 0)
        {
            AppendLog("No displays found.");
            return;
        }
        var screen = screens[Math.Clamp(ScreenPicker.SelectedIndex, 0, screens.Count - 1)];

        var input = new AvaloniaPresenterInput(this);
        _controller = new PresentationController(SelectedDeck,
            (int)(ViewportWidth.Value ?? 1920),
            (int)(ViewportHeight.Value ?? 1080),
            input);
        input.Controller = _controller;
        _controller.Log += (_, message) => AppendLog(message);
        _controller.StateChanged += (_, _) => ShowCurrentSlide();

        _presentationWindow = new PresentationWindow(_controller, screen);
        _presentationWindow.Closed += (_, _) => StopPresentation();
        _presentationWindow.Show();

        _controller.Start();
        SetRunning(true);
    }

    private void StopButton_Click(object? sender, RoutedEventArgs e) => StopPresentation();

    private void AdvanceButton_Click(object? sender, RoutedEventArgs e) => _controller?.Advance();

    private void RewindButton_Click(object? sender, RoutedEventArgs e) => _controller?.Rewind();

    private void ReinitializeButton_Click(object? sender, RoutedEventArgs e) => _controller?.ReinitializeCurrentSlide();

    private void SlideList_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (_controller != null && SlideList.SelectedIndex >= 0)
        {
            _controller.GoToSlide(SlideList.SelectedIndex);
        }
    }

    private void StopPresentation()
    {
        if (_controller == null)
        {
            return;
        }

        var controller = _controller;
        var window = _presentationWindow;
        _controller = null;
        _presentationWindow = null;

        window?.Close();
        controller.Dispose();
        SetRunning(false);
        SlideTitle.Text = "Not running";
        SlideDescription.Text = string.Empty;
        BeatText.Text = string.Empty;
        NotesText.Text = string.Empty;
    }

    private void SetRunning(bool running)
    {
        StartButton.IsEnabled = !running;
        StopButton.IsEnabled = running;
        AdvanceButton.IsEnabled = running;
        RewindButton.IsEnabled = running;
        ReinitializeButton.IsEnabled = running;
        DeckPicker.IsEnabled = !running;
    }

    private void ShowSlideList()
    {
        SlideList.ItemsSource = SelectedDeck.Slides
            .Select((slide, index) => $"{index}. {slide.DisplayName}")
            .ToList();
    }

    private void ShowCurrentSlide()
    {
        if (_controller == null)
        {
            return;
        }

        var index = _controller.CurrentSlideIndex;
        var definition = SelectedDeck.Slides[index];
        var slide = _controller.CurrentSlide;

        SlideList.SelectedIndex = index;
        SlideTitle.Text = $"{index}. {definition.DisplayName}";
        SlideDescription.Text = definition.Description ?? string.Empty;
        BeatText.Text = slide == null ? "Slide failed to start" : $"Beat {slide.BeatIndex}: {slide.CurrentBeat}";
        NotesText.Text = slide?.Notes ?? string.Empty;
    }

    private void AppendLog(string message)
    {
        LogBox.Text += $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
        LogBox.CaretIndex = LogBox.Text.Length;
    }

    private static string DescribeDeck(PresentationDefinition deck) =>
        string.IsNullOrEmpty(deck.Folder) ? deck.Name : $"{deck.Folder} / {deck.Name}";

    private static string DescribeScreen(Screen screen, int index) =>
        $"{index}: {screen.DisplayName ?? "Display"} ({screen.Bounds.Width}×{screen.Bounds.Height}){(screen.IsPrimary ? ", primary" : "")}";
}
