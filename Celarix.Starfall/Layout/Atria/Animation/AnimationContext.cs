using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Animation
{
    public sealed class AnimationContext
    {
        private readonly List<FixedDurationAnimation> _fixedDurationAnimations = [];
        private readonly List<ContinuingAnimation> _continuingAnimations = [];

        public void ScheduleAnimation(FixedDurationAnimation animation)
        {
            _fixedDurationAnimations.Add(animation);
        }
        
        public void ScheduleContinuingAnimation(ContinuingAnimation animation)
        {
            _continuingAnimations.Add(animation);
        }

        public void StaggerAnimations(Queue<Func<FixedDurationAnimation>> animationFactories, int frameDelay,
            Action? onCompleted = null)
        {
            onCompleted ??= () => { };

            var globalFrameRemainder = AtriaLayoutEngine.GlobalFrameNumber % frameDelay;
            var animationCount = animationFactories.Count;
            var staggeredAnimation = ContinuingAnimation.StartNow(() =>
            {
                var currentGlobalFrame = AtriaLayoutEngine.GlobalFrameNumber;
                if ((currentGlobalFrame % frameDelay) == globalFrameRemainder)
                {
                    if (animationFactories.Count != 0)
                    {
                        var nextAnimationFactory = animationFactories.Dequeue();
                        var nextAnimation = nextAnimationFactory();

                        if (animationFactories.Count == 0)
                        {
                            // This is the last animation, so we need to schedule the onCompleted action to run when it finishes.
                            var originalOnCompleted = nextAnimation.OnCompleted;
                            nextAnimation.OnCompleted = () =>
                            {
                                originalOnCompleted?.Invoke();
                                onCompleted();
                            };
                        }

                        ScheduleAnimation(nextAnimation);
                    }
                }
                return animationFactories.Count != 0;
            });
            ScheduleContinuingAnimation(staggeredAnimation);
        }

        public void Update(int currentFrame)
        {
            UpdateFixedDurationAnimations(currentFrame);
            UpdateContinuingAnimations(currentFrame);
        }

        private void UpdateFixedDurationAnimations(int currentFrame)
        {
            // Check if any animations have been added that are not completed but ended before
            // the current frame.
            var alreadyExpiredAnimations = _fixedDurationAnimations
                .Where(a => !a.Completed && a.EndFrame < currentFrame);
            foreach (var animation in alreadyExpiredAnimations)
            {
                // Force end the animation so it does what it needs to do.
                animation.ForceEnd(currentFrame);
            }

            // Animations can schedule more animations, so we need to copy the list to avoid modifying it while iterating.
            var fixedDurationAnimationsCopy = _fixedDurationAnimations.ToArray();
            foreach (var animation in fixedDurationAnimationsCopy)
            {
                // Intentionally leaving update order not strongly defined.
                animation.Update(currentFrame);
            }

            // Remove completed animations.
            _fixedDurationAnimations.RemoveAll(a => a.Completed);
        }

        private void UpdateContinuingAnimations(int currentFrame)
        {
            foreach (var animation in _continuingAnimations)
            {
                // Intentionally leaving update order not strongly defined.
                animation.Update(currentFrame);
            }
            // Remove completed animations.
            _continuingAnimations.RemoveAll(a => a.Completed);
        }

        public static int SecondsToFrames(double seconds)
        {
            // CANIMPROVE: We need to figure out where the frame rate is defined and keep it as some
            // kind of setting or constant somewhere. For now, we'll just hardcode it to 60fps.
            return (int)(seconds * 60);
        }
    }
}
