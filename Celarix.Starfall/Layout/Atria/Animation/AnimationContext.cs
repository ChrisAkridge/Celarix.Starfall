using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Animation
{
    public sealed class AnimationContext
    {
        private readonly List<FixedDurationAnimation> _animations = [];

        public void ScheduleAnimation(FixedDurationAnimation animation)
        {
            _animations.Add(animation);
        }

        public void Update(int currentFrame)
        {
            // Check if any animations have been added that are not completed but ended before
            // the current frame.
            var alreadyExpiredAnimations = _animations
                .Where(a => !a.Completed && a.EndFrame < currentFrame);
            foreach (var animation in alreadyExpiredAnimations)
            {
                // Force end the animation so it does what it needs to do.
                animation.ForceEnd(currentFrame);
            }

            foreach (var animation in _animations)
            {
                // Intentionally leaving update order not strongly defined.
                animation.Update(currentFrame);
            }

            // Remove completed animations.
            _animations.RemoveAll(a => a.Completed);
        }
    }
}
