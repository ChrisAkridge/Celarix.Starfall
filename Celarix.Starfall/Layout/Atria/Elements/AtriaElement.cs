using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Layout.Atria.Basis;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Elements
{
    public abstract class AtriaElement : IAtriaIdentified, ISlideAddable
    {
        private SPointF? _position;
        private Anchor? _anchor;

        public AtriaSlide? Slide { get; set; }
        
        public AtriaId Id { get; protected set; }
        public SPointF Position
        {
            get
            {
                if (_anchor != null)
                {
                    return _anchor.GetPosition(Size);
                }
                return _position ?? SPointF.Zero;
            }
            set
            {
                if (_anchor != null)
                {
                    throw new InvalidOperationException("Cannot set Position directly when an Anchor is applied. Remove the anchor first or modify the anchor instead.");
                }
                _position = value;
            }
        }

        public SSizeF Size { get; set; }
        public double Opacity { get; set; }

        public SRectF Bounds => new SRectF(Position, Size);

        public void Anchor(BasisPoint point, Alignment anchoredPoint)
        {
            _anchor = new Anchor(point, anchoredPoint);
        }

        public void Unanchor()
        {
            _anchor = null;
        }

        public virtual void Update(double deltaTime)
        {
        }

        public abstract void Render(IRenderTarget target);

        public void Animate<TProp>(Expression<Func<AtriaElement, TProp>> propertySelector, Easing easing, double duration, TProp from, TProp to)
        {
            var animation = new ActiveAnimation<TProp>(this, propertySelector, easing, duration, from, to);
            Slide?.AddAnimation(animation);
        }

        public void AnimateTo<TProp>(Expression<Func<AtriaElement, TProp>> propertySelector, Easing easing, double duration, TProp to)
        {
            // featuring the rare back-to-back parentheses
            var currentValue = propertySelector.Compile()(this);
            Animate(propertySelector, easing, duration, currentValue, to);
        }

        public void Animate<TProp>(Expression<Func<AtriaElement, TProp>> propertySelector, double duration, TProp from, TProp to)
        {
            Animate(propertySelector, Easings.Linear, duration, from, to);
        }

        public void AnimateTo<TProp>(Expression<Func<AtriaElement, TProp>> propertySelector, double duration, TProp to)
        {
            AnimateTo(propertySelector, Easings.Linear, duration, to);
        }

        public void AnimateWithDelay<TProp>(Expression<Func<AtriaElement, TProp>> propertySelector, Easing easing, double duration, double delay, TProp from, TProp to)
        {
            var animation = new ActiveAnimation<TProp>(this, propertySelector, easing, duration, from, to)
            {
                Delay = delay
            };
            Slide?.AddAnimation(animation);
        }

        public void AnimateToWithDelay<TProp>(Expression<Func<AtriaElement, TProp>> propertySelector, Easing easing, double duration, double delay, TProp to)
        {
            // aren't expressions fun?
            var currentValue = propertySelector.Compile()(this);
            AnimateWithDelay(propertySelector, easing, duration, delay, currentValue, to);
        }

        public void AnimateWithDelay<TProp>(Expression<Func<AtriaElement, TProp>> propertySelector, double duration, double delay, TProp from, TProp to)
        {
            AnimateWithDelay(propertySelector, Easings.Linear, duration, delay, from, to);
        }

        public void AnimateToWithDelay<TProp>(Expression<Func<AtriaElement, TProp>> propertySelector, double duration, double delay, TProp to)
        {
            AnimateToWithDelay(propertySelector, Easings.Linear, duration, delay, to);
        }

        public void AnchorTopLeftTo(BasisPoint point) => Anchor(point, Alignment.TopLeft);
        public void AnchorTopCenterTo(BasisPoint point) => Anchor(point, Alignment.TopCenter);
        public void AnchorTopRightTo(BasisPoint point) => Anchor(point, Alignment.TopRight);
        public void AnchorLeftCenterTo(BasisPoint point) => Anchor(point, Alignment.LeftCenter);
        public void AnchorCenterTo(BasisPoint point) => Anchor(point, Alignment.Center);
        public void AnchorRightCenterTo(BasisPoint point) => Anchor(point, Alignment.RightCenter);
        public void AnchorBottomLeftTo(BasisPoint point) => Anchor(point, Alignment.BottomLeft);
        public void AnchorBottomCenterTo(BasisPoint point) => Anchor(point, Alignment.BottomCenter);
        public void AnchorBottomRightTo(BasisPoint point) => Anchor(point, Alignment.BottomRight);
    }
}
