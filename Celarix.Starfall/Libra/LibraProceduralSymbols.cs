using Celarix.Starfall.Libra.Renderables;
using Celarix.Starfall.Libra.Renderables.Path;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Libra
{
    internal static class LibraProceduralSymbols
    {
        public static LibraPathRenderable LeftFence(FenceType fenceType,
            LibraRenderingContext context,
            PathStyle pathStyle,
            Guid renderableKeyGuid,
            string renderableKeyRole)
        {
            return fenceType switch
            {
                FenceType.Parentheses => LeftParentheses(context, pathStyle, renderableKeyGuid, renderableKeyRole),
                _ => throw new NotImplementedException($"Left fence of type {fenceType} is not implemented.")
            };
        }

        public static LibraPathRenderable RightFence(FenceType fenceType,
            LibraRenderingContext context,
            PathStyle pathStyle,
            Guid renderableKeyGuid,
            string renderableKeyRole)
        {
            return fenceType switch
            {
                FenceType.Parentheses => RightParentheses(context, pathStyle, renderableKeyGuid, renderableKeyRole),
                _ => throw new NotImplementedException($"Right fence of type {fenceType} is not implemented.")
            };
        }

        public static LibraPathRenderable LeftParentheses(LibraRenderingContext context,
            PathStyle pathStyle,
            Guid renderableKeyGuid,
            string renderableKeyRole)
        {
            var pMetrics = context.Metrics.Fences.ParenthesesMetrics;

            var w = pMetrics.ParenthesesWidthEm * context.Em;
            var h = pMetrics.ParenthesesHeightEm * context.Em;
            var t = pMetrics.EndThicknessEm * context.Em;

            var outerTop = new SPointF(w - t, 0);
            var innerTop = new SPointF(w, 0);
            var innerBottom = new SPointF(w, h);
            var outerBottom = new SPointF(w - t, h);

            var innerControl1 = new SPointF(
                w - (pMetrics.InnerBulge * w),
                h * pMetrics.UpperControlYFraction
            );
            var innerControl2 = new SPointF(
                w - (pMetrics.InnerBulge * w),
                h * pMetrics.LowerControlYFraction
            );

            var outerControl1 = new SPointF(
                w - (pMetrics.OuterBulge * w),
                h * pMetrics.LowerControlYFraction
            );

            var outerControl2 = new SPointF(
                w - (pMetrics.OuterBulge * w),
                h * pMetrics.UpperControlYFraction
            );

            var libraPath = new LibraPathBuilder()
                .MoveTo(outerTop.X, outerTop.Y)
                .LineTo(innerTop.X, innerTop.Y)
                .CubicTo(innerControl1.X, innerControl1.Y,
                    innerControl2.X, innerControl2.Y,
                    innerBottom.X, innerBottom.Y)
                .LineTo(outerBottom.X, outerBottom.Y)
                .CubicTo(outerControl1.X, outerControl1.Y,
                    outerControl2.X, outerControl2.Y,
                    outerTop.X, outerTop.Y)
                .ClosePath();
            return new LibraPathRenderable(new LibraRenderableKey(renderableKeyGuid, renderableKeyRole), libraPath, pathStyle)
            {
                Size = new SSizeF(w, h)
            };
        }

        public static LibraPathRenderable RightParentheses(LibraRenderingContext context,
            PathStyle pathStyle,
            Guid renderableKeyGuid,
            string renderableKeyRole)
        {
            var leftParentheses = LeftParentheses(context, pathStyle, renderableKeyGuid, renderableKeyRole);
            return leftParentheses.MirrorHorizontally(context.Em * context.Metrics.Fences.ParenthesesMetrics.ParenthesesWidthEm);
        }
    }
}
