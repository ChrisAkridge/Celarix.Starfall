using Celarix.Starfall.Layout.Atria;
using Celarix.Starfall.Layout.Atria.Animation;
using Celarix.Starfall.Mathematics;
using Celarix.Starfall.Rendering.Models;
using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Playground.MathFun
{
    internal sealed class LayeredProblemStack
    {
        private readonly int _width;
        private readonly int _height;

        private List<SColor> _backgroundColors = [];
        private int _selectedProblemIndex = 0;

        public int SelectedProblemIndex => _selectedProblemIndex;
        public SColor SelectedProblemColor => _backgroundColors[_selectedProblemIndex];

        public LayeredProblemStack(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public void AddProblem(SColor backgroundColor)
        {
            _backgroundColors.Add(backgroundColor);
        }

        public void SwitchToProblem(int index,
            AnimationContext context,
            AtriaLayer layer)
        {
            if (_selectedProblemIndex == index)
            {
                return;
            }
            else if (index < 0 || index >= _backgroundColors.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            _selectedProblemIndex = index;
            var problemY = YOffsetForProblem(index);
            var currentY = layer.Transform.Translation.Y;
            context.ScheduleAnimation(FixedDurationAnimation.StartNow(AnimationContext.SecondsToFrames(1d),
                p =>
                {
                    var eased = Easings.Smoothstep(p);
                    var y = currentY + ((problemY - currentY) * eased);

                    layer.Transform = layer.Transform with
                    {
                        Translation = layer.Transform.Translation.WithY(y)
                    };
                }));
        }

        public double YOffsetForProblem(int index) => _height * index;

        public void RenderBackgrounds(IRenderTarget target)
        {
            for (var i = 0; i < _backgroundColors.Count; i++)
            {
                var yOffset = YOffsetForProblem(i);
                target.DrawRectangle(new(0, yOffset, _width, _height), _backgroundColors[i],
                    SPaintStyle.Fill, SAngle.Zero);
            }
        }
    }
}
