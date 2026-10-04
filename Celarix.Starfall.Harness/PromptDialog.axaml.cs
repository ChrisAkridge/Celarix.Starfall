using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Celarix.Starfall.Harness;

/// <summary>
/// A small modal prompt. In text mode it returns the typed text; in yes/no mode the buttons read
/// Yes and No and it returns "y" or null.
/// </summary>
public partial class PromptDialog : Window
{
    public PromptDialog()
    {
        InitializeComponent();
    }

    public static PromptDialog ForText(string prompt)
    {
        var dialog = new PromptDialog();
        dialog.PromptText.Text = prompt;
        dialog.Opened += (_, _) => dialog.AnswerBox.Focus();
        return dialog;
    }

    public static PromptDialog ForYesNo(string prompt)
    {
        var dialog = new PromptDialog();
        dialog.PromptText.Text = prompt;
        dialog.AnswerBox.IsVisible = false;
        dialog.OkButton.Content = "Yes";
        dialog.CancelButton.Content = "No";
        return dialog;
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e) =>
        Close(AnswerBox.IsVisible ? AnswerBox.Text ?? string.Empty : "y");

    private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(null);
}
