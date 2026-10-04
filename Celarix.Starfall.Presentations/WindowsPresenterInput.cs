using Celarix.Starfall.Atria;

namespace Celarix.Starfall.Presentations
{
    /// <summary>
    /// Console presenter input, except files are picked with the Windows open-file dialog.
    /// </summary>
    internal sealed class WindowsPresenterInput : ConsolePresenterInput
    {
        public override string? AskForFile(string prompt)
        {
            using var openFileDialog = new OpenFileDialog { Title = prompt };
            return openFileDialog.ShowDialog() == DialogResult.OK ? openFileDialog.FileName : null;
        }
    }
}
