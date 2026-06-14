using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Rendering;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria
{
    public abstract class AtriaSlide
    {
        private readonly List<AtriaElement> _elements = new();
        private readonly List<BasisElement> _basisElements = new();
        private readonly List<ActiveAnimation> _activeAnimations = new();

        public MeasurementService MeasurementService { get; private set; }
        public DebugMode DebugMode { get; private set; }
        public SColor BackgroundColor { get; set; }
        public SSizeF Size { get; }

        protected IReadOnlyList<AtriaElement> Elements => _elements;
        protected IReadOnlyList<BasisElement> BasisElements => _basisElements;

        // Points
        public SPointF TopLeft => SPointF.Zero;
        public SPointF TopCenter => new SPointF(Size.Width / 2, 0);
        public SPointF TopRight => new SPointF(Size.Width, 0);
        public SPointF LeftCenter => new SPointF(0, Size.Height / 2);
        public SPointF Center => new SPointF(Size.Width / 2, Size.Height / 2);
        public SPointF RightCenter => new SPointF(Size.Width, Size.Height / 2);
        public SPointF BottomLeft => new SPointF(0, Size.Height);
        public SPointF BottomCenter => new SPointF(Size.Width / 2, Size.Height);
        public SPointF BottomRight => new SPointF(Size.Width, Size.Height);

        public AtriaSlide(int width, int height)
        {
            Size = new SSizeF(width, height);
        }

        internal void SetProtectedProperties(MeasurementService measurementService, DebugMode debugMode)
        {
            MeasurementService = measurementService;
            DebugMode = debugMode;
        }

        public abstract void Initialize();

        public virtual void Update(double deltaTime)
        {
            for (int i = _activeAnimations.Count - 1; i >= 0; i--)
            {
                var animation = _activeAnimations[i];
                animation.Update(deltaTime);
                if (animation.IsCompleted)
                {
                    _activeAnimations.RemoveAt(i);
                }
            }

            foreach (var element in _elements)
            {
                element.Update(deltaTime);
            }
        }

        public virtual void Render(IRenderTarget target)
        {
            target.Clear(BackgroundColor);
            foreach (var element in _elements)
            {
                element.Render(target);
            }
        }

        public virtual SlideAdvanceResult Rewind()
        {
            // Basic implementation assuming no state machine.
            return SlideAdvanceResult.CanRewind;
        }

        public virtual SlideAdvanceResult Advance()
        {
            // Basic implementation assuming no state machine.
            return SlideAdvanceResult.CanAdvance;
        }

        public AddedElementOptions Add(IEnumerable<ISlideAddable> addables)
        {
            var newAddables = addables.ToArray();
            var newElements = new List<AtriaElement>();
            var newBasisElements = new List<BasisElement>();
            foreach (var addable in newAddables)
            {
                addable.Slide = this;
                if (addable is AtriaElement element)
                {
                    _elements.Add(element);
                    newElements.Add(element);
                }
                else if (addable is BasisElement basisElement)
                {
                    _basisElements.Add(basisElement);
                    newBasisElements.Add(basisElement);
                }
            }
            return new AddedElementOptions(this, [.. newElements], [.. newBasisElements]);
        }

        public void Remove(IEnumerable<ISlideAddable> removeables)
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

        public IReadOnlyList<AtriaElement> QueryMultiple(params IEnumerable<string> selectors)
        {
            var matchedElements = new List<AtriaElement>();
            foreach (var selector in selectors)
            {
                matchedElements.AddRange(Query(selector));
            }
            return matchedElements;
        }

        public IReadOnlyList<AtriaElement> Query(string selector)
        {
            var matchedElements = new List<AtriaElement>();
            foreach (var element in _elements)
            {
                if (element.Id.Matches(selector))
                {
                    matchedElements.Add(element);
                }
            }
            return matchedElements;
        }

        public IReadOnlyList<BasisElement> QueryBasis(string selector)
        {
            var matchedElements = new List<BasisElement>();
            foreach (var element in _basisElements)
            {
                if (element.Id.Matches(selector))
                {
                    matchedElements.Add(element);
                }
            }
            return matchedElements;
        }

        internal void AddAnimation(ActiveAnimation animation)
        {
            _activeAnimations.Add(animation);
        }
    }
}
