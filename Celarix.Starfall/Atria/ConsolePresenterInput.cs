namespace Celarix.Starfall.Atria;

/// <summary>
/// Presenter input read from the console.
/// </summary>
public class ConsolePresenterInput : IPresenterInput
{
    public virtual string? AskForText(string prompt)
    {
        Console.Write($"{prompt} ");
        return Console.ReadLine();
    }

    public virtual bool AskYesNo(string prompt)
    {
        Console.Write($"{prompt} (y/n): ");
        return Console.ReadLine()?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true;
    }

    public virtual string? AskForFile(string prompt)
    {
        Console.Write($"{prompt} (enter a path, or leave blank to cancel): ");
        var path = Console.ReadLine()?.Trim().Trim('"');
        return string.IsNullOrEmpty(path) ? null : path;
    }
}
