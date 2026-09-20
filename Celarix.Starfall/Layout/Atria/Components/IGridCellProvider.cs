using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Layout.Atria.Components
{
    public interface IGridCellProvider
    {
        SColor? GetCellColor(SPoint cell) => null;

        string? GetText(SPoint cell) => null;

        SFont? GetFont(SPoint cell) => null;

        SColor? GetTextColor(SPoint cell) => null;
    }
}
