using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;

namespace Celarix.Starfall.Layout.Atria
{
    public sealed class AtriaLayer
    {
        private readonly LayeredAtriaSlide _slide;
        private readonly List<AtriaElement> _elements = new();
        private readonly List<BasisElement> _basisElements = new();

        internal AtriaLayer(LayeredAtriaSlide slide)
        {
            _slide = slide;
        }

        public STransform2D Transform { get; set; } = STransform2D.Identity;

        public IReadOnlyList<AtriaElement> Elements => _elements;

        public IReadOnlyList<BasisElement> BasisElements => _basisElements;

        public AddedElementOptions Add(IEnumerable<ISlideAddable> addables) =>
            _slide.Add(this, addables);

        public AddedElementOptions Add(ISlideAddable addable) =>
            Add([addable]);

        internal void AddElement(AtriaElement element)
        {
            _elements.Add(element);
        }

        internal void AddBasisElement(BasisElement basisElement)
        {
            _basisElements.Add(basisElement);
        }

        internal void Remove(IEnumerable<ISlideAddable> removeables)
        {
            foreach (var removeable in removeables)
            {
                if (removeable is AtriaElement element)
                {
                    _elements.Remove(element);
                }
                else if (removeable is BasisElement basisElement)
                {
                    _basisElements.Remove(basisElement);
                }
            }
        }
    }
}
