using Celarix.Starfall.Layout.Atria.Elements;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Animation
{
    public abstract class AnimationType<TProp>
    {
        public abstract Expression<Func<AtriaElement, TProp>> Property { get; }
        public TProp From { get; protected set; }
        public TProp To { get; protected set; }
    }

    public static class AnimationTypes
    {
        public static FadeIn FadeIn => new FadeIn();
    }

    public sealed class FadeIn : AnimationType<double>
    {
        public override Expression<Func<AtriaElement, double>> Property => element => element.Opacity;

        public FadeIn() : base()
        {
            From = 0d;
            To = 1d;
        }
    }
}
