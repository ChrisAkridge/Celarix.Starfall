using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria
{
    public sealed class AddedElementOptions
    {
        private readonly AtriaSlide _slide;
        private readonly AtriaElement[] _elements;
        private readonly BasisElement[] _basisElements;

        internal AddedElementOptions(AtriaSlide slide, AtriaElement[] elements, BasisElement[] basisElements)
        {
            _slide = slide;
            _elements = elements;
            _basisElements = basisElements;
        }

        public AddedElementOptions AnimateBasic<TProp>(double duration, AnimationType<TProp> animationType, Easing easing)
        {
            foreach (var element in _elements)
            {
                element.Animate(animationType.Property, easing, duration, animationType.From, animationType.To);
            }

            return this;
        }
    }
}
