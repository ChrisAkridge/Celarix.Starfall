using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.Starfall.Layout.Atria
{
    public abstract class LayeredAtriaSlide : AtriaSlide
    {
        private readonly List<AtriaLayer> _orderedLayers;

        protected LayeredAtriaSlide(int width, int height, int layerCount) : base(width, height)
        {
            if (layerCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(layerCount), "A layered slide must have at least one layer.");
            }

            _orderedLayers = Enumerable.Range(0, layerCount)
                .Select(_ => new AtriaLayer(this))
                .ToList();
        }

        protected IReadOnlyList<AtriaLayer> OrderedLayers => _orderedLayers;

        protected AtriaLayer FrontmostLayer => _orderedLayers[^1];

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="Render(IRenderTarget)"/> draws its own
        /// background color. Set this to false in order to draw your own background in a derived class.
        /// Defaults to <see cref="true"/>.
        /// </summary>
        protected bool RenderOwnBackground { get; set; } = true;

        internal AddedElementOptions Add(AtriaLayer layer, IEnumerable<ISlideAddable> addables)
        {
            var newAddables = addables.ToArray();
            var options = AddCore(newAddables);

            foreach (var addable in newAddables)
            {
                if (addable is AtriaElement element)
                {
                    layer.AddElement(element);
                }
                else if (addable is BasisElement basisElement)
                {
                    layer.AddBasisElement(basisElement);
                }
            }

            return options;
        }

        public override AddedElementOptions Add(IEnumerable<ISlideAddable> addables)
        {
            WriteFallbackLayerWarning();
            return Add(FrontmostLayer, addables);
        }

        public override void Remove(IEnumerable<ISlideAddable> removeables)
        {
            var removeableArray = removeables.ToArray();
            base.Remove(removeableArray);

            foreach (var layer in _orderedLayers)
            {
                layer.Remove(removeableArray);
            }
        }

        public override void Render(IRenderTarget target)
        {
            if (RenderOwnBackground)
            {
                target.Clear(BackgroundColor);
            }

            foreach (var layer in _orderedLayers)
            {
                target.PushTransform(layer.Transform);
                try
                {
                    foreach (var element in layer.Elements)
                    {
                        element.Render(target);
                    }

                    if (DebugMode.ShowAnchors)
                    {
                        foreach (var basisElement in layer.BasisElements)
                        {
                            basisElement.RenderDebug(target);
                        }
                    }
                }
                finally
                {
                    target.PopTransform();
                }
            }
        }

        private static void WriteFallbackLayerWarning()
        {
            var previousForegroundColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning: Add(...) was called on a layered Atria slide without a layer. The frontmost layer was used.");
            }
            finally
            {
                Console.ForegroundColor = previousForegroundColor;
            }
        }
    }

    public abstract class LayeredAtriaSlide<TLayer> : LayeredAtriaSlide
        where TLayer : struct, Enum
    {
        private readonly IReadOnlyDictionary<TLayer, AtriaLayer> _layers;

        protected LayeredAtriaSlide(int width, int height) : base(width, height, Enum.GetValues<TLayer>().Length)
        {
            var orderedLayerKeys = Enum.GetValues<TLayer>()
                .OrderBy(layer => layer)
                .ToArray();

            _layers = orderedLayerKeys
                .Select((layer, index) => new { layer, atriaLayer = OrderedLayers[index] })
                .ToDictionary(item => item.layer, item => item.atriaLayer);
        }

        protected IReadOnlyDictionary<TLayer, AtriaLayer> Layers => _layers;

        protected AtriaLayer this[TLayer layer] => _layers[layer];
    }
}
