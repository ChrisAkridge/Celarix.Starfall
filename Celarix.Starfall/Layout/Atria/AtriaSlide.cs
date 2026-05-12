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

        internal void AddAnimation(ActiveAnimation animation)
        {
            _activeAnimations.Add(animation);
        }
    }
}
