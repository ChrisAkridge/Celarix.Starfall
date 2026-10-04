using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Celarix.Starfall.Atria;

namespace Celarix.Starfall.Harness;

/// <summary>
/// Presenter input shown as dialogs over the operator window. Slides ask synchronously from
/// inside Advance, so each dialog runs a nested dispatcher frame until it closes, and frames are
/// paused meanwhile, just as the console prompt used to pause the presentation.
/// </summary>
internal sealed class AvaloniaPresenterInput : IPresenterInput
{
    private readonly Window _owner;

    public AvaloniaPresenterInput(Window owner)
    {
        _owner = owner;
    }

    public PresentationController? Controller { get; set; }

    public string? AskForText(string prompt) =>
        WaitFor(PromptDialog.ForText(prompt).ShowDialog<string?>(_owner));

    public bool AskYesNo(string prompt) =>
        WaitFor(PromptDialog.ForYesNo(prompt).ShowDialog<string?>(_owner)) != null;

    public string? AskForFile(string prompt)
    {
        var files = WaitFor(_owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = prompt,
            AllowMultiple = false
        }));
        return files.Count == 0 ? null : files[0].TryGetLocalPath();
    }

    private T WaitFor<T>(Task<T> task)
    {
        var wasPaused = Controller?.IsPaused ?? false;
        if (Controller != null)
        {
            Controller.IsPaused = true;
        }

        try
        {
            if (!task.IsCompleted)
            {
                var frame = new DispatcherFrame();
                task.ContinueWith(_ => Dispatcher.UIThread.Post(() => frame.Continue = false));
                Dispatcher.UIThread.PushFrame(frame);
            }

            return task.GetAwaiter().GetResult();
        }
        finally
        {
            if (Controller != null)
            {
                Controller.IsPaused = wasPaused;
            }
        }
    }
}
