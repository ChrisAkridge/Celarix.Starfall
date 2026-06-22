using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;

namespace Celarix.Starfall.Layout.Atria.Elements;

public class MultilineTextBlock : AtriaElement
{
    private int _lastRenderedLineCount = 0;

    public string Text { get; set; }
    public SFont Font { get; set; }
    public SColor Color { get; set; }
    public double LineSpacing { get; set; } = 1.2; // 120% of font size
    public Alignment HorizontalAlignment { get; set; } = Alignment.LeftCenter;

    public MultilineTextBlock(string atriaIdString)
    {
        Id = AtriaId.Parse(atriaIdString);
    }
    
    public override void Render(IRenderTarget target)
    {
        var lines = Text.Split('\n');
        var lineHeight = (Font.Size ?? 12f) * LineSpacing;

        if (_lastRenderedLineCount != lines.Length)
        {
            // CANIMPROVE: We also need to check the widths of each line so right-anchoring works
            var lineMeasurements = lines.Select(line =>
            {
                SSizeF measuredLine = target.MeasureText(line, Font);
                return new SSizeF(measuredLine.Width, measuredLine.Height * LineSpacing);
            });
            Size = new SSizeF(lineMeasurements.Max(m => m.Width), lineMeasurements.Sum(m => m.Height));
            _lastRenderedLineCount = lines.Length;
        }

        var y = Position.Y;
        
        foreach (var line in lines)
        {
            var lineRect = new SRectF(Position.X, y, Size.Width, lineHeight);
            target.DrawTextDirectly(line, Font, lineRect, Color.WithOpacity(Opacity), SAngle.Zero);
            y += lineHeight;
        }
    }
}