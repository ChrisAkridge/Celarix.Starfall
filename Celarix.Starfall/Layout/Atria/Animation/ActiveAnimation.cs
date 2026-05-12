using Celarix.Starfall.Layout.Atria.Elements;
using Celarix.Starfall.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Animation
{
    internal abstract class ActiveAnimation
    {
        public abstract bool IsCompleted { get; }
        public abstract void Update(double deltaTime);
    }

    internal sealed class ActiveAnimation<TProp> : ActiveAnimation
    {
        private readonly IInterpolator<TProp> _interpolator = Interpolators.Get<TProp>();
        private double _elapsedTime;
        private double _startTime;
        private double _endTime;
        private Expression<Action<AtriaElement, TProp>> _setterExpression;
        private Action<AtriaElement, TProp> _setter;

        public AtriaElement Element { get; }
        public Expression<Func<AtriaElement, TProp>> PropertySelector { get; }
        public Easing Easing { get; }
        public double Duration { get; }
        public double? Delay { get; init; }
        public TProp From { get; }
        public TProp To { get; }
        public override bool IsCompleted => _elapsedTime >= Duration;
        public ActiveAnimation(AtriaElement element, Expression<Func<AtriaElement, TProp>> propertySelector, Easing easing, double duration, TProp from, TProp to, double? delay = null)
        {
            Element = element;
            PropertySelector = propertySelector;
            Easing = easing;
            Duration = duration;
            From = from;
            To = to;
            Delay = delay;

            _startTime = delay ?? 0;
            _endTime = _startTime + duration;
            _setterExpression = ActiveAnimation<TProp>.CreateSetterExpression(propertySelector);
            _setter = _setterExpression.Compile();
        }

        public override void Update(double deltaTime)
        {
            _elapsedTime += deltaTime;
            if (_elapsedTime < _startTime)
            {
                return; // Not started yet
            }

            double progress = Math.Min((_elapsedTime - _startTime) / Duration, 1);
            double easedProgress = Easing(progress);
            TProp currentValue = _interpolator.Interpolate(From, To, easedProgress);
            SetProperty(currentValue);
        }

        private static Expression<Action<AtriaElement, TProp>> CreateSetterExpression(Expression<Func<AtriaElement, TProp>> propertySelector)
        {
            if (propertySelector.Body is MemberExpression memberExpr && memberExpr.Member is System.Reflection.PropertyInfo propInfo)
            {
                var parameter = Expression.Parameter(typeof(TProp), "value");
                System.Reflection.MethodInfo setMethod = propInfo.GetSetMethod() ?? throw new InvalidOperationException("The property must have a setter.");
                var setterCall = Expression.Call(
                    Expression.Convert(propertySelector.Parameters[0], typeof(AtriaElement)),
                    setMethod,
                    parameter
                );
                return Expression.Lambda<Action<AtriaElement, TProp>>(setterCall, propertySelector.Parameters[0], parameter);
            }
            throw new InvalidOperationException("Property selector must be a simple property access.");
        }

        private void SetProperty(TProp currentValue)
        {
            _setter(Element, currentValue);
        }
    }
}
