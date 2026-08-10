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
    public abstract class AtriaElement : IAtriaIdentified, ISlideAddable, IDisposable
    {
        private SPointF? _position;
        private Anchor? _anchor;
        private AnimationContext? _animations;
        private bool _disposed;

        protected Alignment? AnchoredPosition
        {
            get
            {
                if (_anchor == null) { return null; }
                return _anchor.AnchoredPoint;
            }
        }

        public AtriaSlide? Slide { get; set; }
        protected AnimationContext Animations => _animations ??= (Slide?.AnimationContexts.CreateFor(this)
            ?? throw new InvalidOperationException("Slide must be set before creating an animation context."));
        
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
            SchedulePropertyAnimation(propertySelector, easing, duration, delay: 0d, from, to);
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
            SchedulePropertyAnimation(propertySelector, easing, duration, delay, from, to);
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

        private void SchedulePropertyAnimation<TProp>(Expression<Func<AtriaElement, TProp>> propertySelector,
            Easing easing,
            double duration,
            double delay,
            TProp from,
            TProp to)
        {
            var interpolator = Interpolators.Get<TProp>();
            var setter = CreateSetterExpression(propertySelector).Compile();
            var durationFrames = Math.Max(1, AnimationContext.SecondsToFrames(duration));
            var delayFrames = Math.Max(0, AnimationContext.SecondsToFrames(delay));
            var animation = FixedDurationAnimation.StartIn(delayFrames, durationFrames, progress =>
            {
                var easedProgress = easing(progress);
                var currentValue = interpolator.Interpolate(from, to, easedProgress);
                setter(this, currentValue);
            });

            Animations.ScheduleAnimation(animation);
        }

        private static Expression<Action<AtriaElement, TProp>> CreateSetterExpression<TProp>(Expression<Func<AtriaElement, TProp>> propertySelector)
        {
            if (propertySelector.Body is MemberExpression memberExpr && memberExpr.Member is System.Reflection.PropertyInfo propInfo)
            {
                var parameter = Expression.Parameter(typeof(TProp), "value");
                var setMethod = propInfo.GetSetMethod() ?? throw new InvalidOperationException("The property must have a setter.");
                var setterCall = Expression.Call(
                    Expression.Convert(propertySelector.Parameters[0], typeof(AtriaElement)),
                    setMethod,
                    parameter
                );
                return Expression.Lambda<Action<AtriaElement, TProp>>(setterCall, propertySelector.Parameters[0], parameter);
            }
            throw new InvalidOperationException("Property selector must be a simple property access.");
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

        public virtual void Dispose()
        {
            if (_disposed) { return; }
            _animations?.Dispose();
            _disposed = true;
        }
    }
}
