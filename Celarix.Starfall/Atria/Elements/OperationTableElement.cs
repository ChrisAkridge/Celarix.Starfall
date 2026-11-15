using Celarix.Starfall.Mathematics.Schoolbook;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;

namespace Celarix.Starfall.Atria.Elements;

/// <summary>
/// The single-digit addition or multiplication table (0 to base − 1 on each side, with headers),
/// highlighting one looked-up fact: its row, its column, and the cell where they meet.
/// </summary>
/// <remarks>
/// Pairs with <see cref="SchoolbookArithmeticElement"/>: set <see cref="Lookup"/> from a step's
/// <see cref="SchoolbookStep.Lookups"/> to show where each fact comes from. An addition lookup with
/// a carry shifts the column headers by the carry, as if it were already added (1 through 10 for a
/// carried 1, 14 through 23 for a running total of 14).
/// </remarks>
public sealed class OperationTableElement : AtriaElement
{
    private TableLookup? _lookup;

    public OperationTableElement(string atriaIdString, TableOperation operation = TableOperation.Addition, int numberBase = 10)
    {
        if (numberBase < 2 || numberBase > 16) { throw new ArgumentOutOfRangeException(nameof(numberBase)); }
        Id = AtriaId.Parse(atriaIdString);
        Operation = operation;
        NumberBase = numberBase;
        Opacity = 1d;
    }

    public TableOperation Operation { get; set; }
    public int NumberBase { get; }

    /// <summary>Gets or sets how far addition columns are shifted by a carry. 0 is the plain table.</summary>
    public int Carry { get; set; }

    /// <summary>
    /// Gets or sets the fact to highlight, or null for none. Setting a lookup also switches the
    /// table to its operation and carry.
    /// </summary>
    public TableLookup? Lookup
    {
        get => _lookup;
        set
        {
            _lookup = value;
            if (value is { } lookup)
            {
                Operation = lookup.Operation;
                Carry = lookup.Operation == TableOperation.Addition ? lookup.Carry : 0;
            }
        }
    }

    public string FontFamily { get; set; } = "Cambria Math";

    public SColor HeaderBackground { get; set; } = new(40, 30, 170, 255);
    public SColor CellBackground { get; set; } = new(20, 12, 150, 255);
    public SColor LineColor { get; set; } = new(70, 60, 200, 255);
    public SColor HeaderTextColor { get; set; } = new(255, 170, 80, 255);
    public SColor CellTextColor { get; set; } = new(200, 200, 235, 255);
    public SColor CrossBackground { get; set; } = new(255, 255, 255, 45);
    public SColor HitBackground { get; set; } = new(255, 230, 90, 255);
    public SColor HitTextColor { get; set; } = new(20, 12, 100, 255);

    public override void Render(IRenderTarget target)
    {
        if (Size.Width <= 0 || Size.Height <= 0 || Opacity <= 0) { return; }

        var count = NumberBase + 1;
        var cell = Math.Min(Size.Width, Size.Height) / count;
        var left = Position.X + (Size.Width - cell * count) / 2;
        var top = Position.Y + (Size.Height - cell * count) / 2;
        var shift = Operation == TableOperation.Addition ? Carry : 0;

        var font = new SFontFamily(FontFamily, null);
        var fontSize = target.FitTextToHeight("0", font, (float)(cell * 0.55));
        var widest = Operation == TableOperation.Addition
            ? Format(2 * (NumberBase - 1) + Math.Max(1, shift))
            : Format((NumberBase - 1) * (NumberBase - 1));
        // Size for at least three digits, so the text doesn't jump as carries shift the table.
        fontSize = Math.Min(fontSize, target.FitTextToWidth(widest.PadLeft(3, '0'), font, (float)(cell * 0.8)));
        var sizedFont = font.WithSize(fontSize);

        SRectF CellRect(int column, int row) => new(left + column * cell, top + row * cell, cell, cell);

        var lookup = _lookup;
        for (var row = 0; row < count; row++)
        {
            for (var column = 0; column < count; column++)
            {
                var isHeader = row == 0 || column == 0;
                var x = column - 1;
                var y = row - 1;
                var inCross = lookup is { } l && ((column > 0 && x == l.X) || (row > 0 && y == l.Y));
                var isHit = lookup is { } h && x == h.X && y == h.Y;

                var rect = CellRect(column, row);
                target.DrawRectangle(rect, Fade(isHeader ? HeaderBackground : CellBackground), SPaintStyle.Fill, SAngle.Zero);
                if (isHit)
                {
                    target.DrawRectangle(rect, Fade(HitBackground), SPaintStyle.Fill, SAngle.Zero);
                }
                else if (inCross)
                {
                    target.DrawRectangle(rect, Fade(CrossBackground), SPaintStyle.Fill, SAngle.Zero);
                }

                string text;
                if (row == 0 && column == 0) { text = Operation == TableOperation.Addition ? "+" : "×"; }
                else if (row == 0) { text = Format(x + shift); }
                else if (column == 0) { text = Format(y); }
                else { text = Format(Operation == TableOperation.Addition ? x + shift + y : x * y); }

                var color = isHit ? HitTextColor : isHeader ? HeaderTextColor : CellTextColor;
                target.DrawText(text, sizedFont, rect, Fade(color), SAngle.Zero);
            }
        }

        var thickness = (float)Math.Max(1d, cell * 0.03);
        for (var i = 0; i <= count; i++)
        {
            var offset = i * cell;
            target.DrawLine(new SPointF(left + offset, top), new SPointF(left + offset, top + count * cell), Fade(LineColor), thickness);
            target.DrawLine(new SPointF(left, top + offset), new SPointF(left + count * cell, top + offset), Fade(LineColor), thickness);
        }
    }

    private string Format(int value)
    {
        const string alphabet = "0123456789ABCDEF";
        if (value == 0) { return "0"; }
        var text = "";
        while (value > 0)
        {
            text = alphabet[value % NumberBase] + text;
            value /= NumberBase;
        }
        return text;
    }

    private SColor Fade(SColor color) => new(color.R, color.G, color.B, (byte)(color.A * Math.Clamp(Opacity, 0d, 1d)));
}
