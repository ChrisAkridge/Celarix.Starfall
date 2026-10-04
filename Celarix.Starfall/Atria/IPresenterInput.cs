namespace Celarix.Starfall.Atria;

/// <summary>
/// Asks the presenter for input while a presentation runs, such as a number from the audience.
/// Slides should go through this instead of the console so hosts can show dialogs or replay
/// recorded answers.
/// </summary>
public interface IPresenterInput
{
    /// <summary>
    /// Asks for a line of text. Returns null if the presenter cancels.
    /// </summary>
    string? AskForText(string prompt);

    /// <summary>
    /// Asks a yes/no question.
    /// </summary>
    bool AskYesNo(string prompt);

    /// <summary>
    /// Asks for a file to open. Returns null if the presenter cancels.
    /// </summary>
    string? AskForFile(string prompt);
}
