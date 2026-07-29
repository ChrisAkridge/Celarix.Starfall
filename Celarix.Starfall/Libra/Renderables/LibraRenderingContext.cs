using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;

namespace Celarix.Starfall.Libra.Renderables
{
    public sealed class LibraRenderingContext
    {
        public MeasurementService MeasurementService { get; }
        public SFont Font { get; }
        public SFontMetrics FontMetrics { get; }
        public double Em { get; }

        public double BaselineY => -FontMetrics.Ascent;

        public double FontHeight =>
            FontMetrics.Descent - FontMetrics.Ascent;

        public LibraRenderingContext(MeasurementService measurementService, SFont font)
        {
            MeasurementService = measurementService;
            Font = font;
            FontMetrics = measurementService.GetFontMetrics(font);
            Em = measurementService.MeasureText("M", font).Height;
        }

        public LibraRenderingContext ScaleFont(double scaleFactor)
        {
            var scaledFont = Font.WithSize((Font.Size ?? 12f) * (float)scaleFactor);
            return new LibraRenderingContext(MeasurementService, scaledFont);
        }

        public double ScaleEm(double scaleFactor)
        {
            return Em * scaleFactor;
        }
    }
}
