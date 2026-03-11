using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Helium.Elements
{
    public abstract class ResizableHeliumElement : HeliumElement
    {
        protected double desiredWidthFraction = Constants.DefaultSize;
        protected double desiredHeightFraction = Constants.DefaultSize;

        public override double DesiredWidthFraction => desiredWidthFraction;

        public override double DesiredHeightFraction => desiredHeightFraction;

        public override void MeasureSelf(SSizeF maxSize, MeasurementService measurementService)
        {
            var width = maxSize.Width * (double)DesiredWidthFraction;
            var height = maxSize.Height * (double)DesiredHeightFraction;
            ActualSize = new SSizeF(width, height);
        }

        public virtual void SetDesiredWidthFraction(double widthFraction)
        {
            desiredWidthFraction = widthFraction;
        }

        public virtual void SetDesiredHeightFraction(double heightFraction)
        {
            desiredHeightFraction = heightFraction;
        }
    }
}
