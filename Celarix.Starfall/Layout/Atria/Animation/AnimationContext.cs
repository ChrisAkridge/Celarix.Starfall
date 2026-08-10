using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Animation
{
    public sealed class AnimationContext : IDisposable
    {
        private readonly AnimationContextRegistry? _registry;
        private readonly List<FixedDurationAnimation> _fixedDurationAnimations = [];
        private readonly List<ContinuingAnimation> _continuingAnimations = [];
        private readonly List<AnimationSlot> _slots = [];
        private int? _lastUpdatedFrame;
        private bool _disposed;

        public object? Owner { get; }
        public bool IsAnimating => _fixedDurationAnimations.Any(a => !a.Completed)
            || _continuingAnimations.Any(a => !a.Completed);
        public int RunningAnimationCount => _fixedDurationAnimations.Count(a => !a.Completed)
            + _continuingAnimations.Count(a => !a.Completed);

        public AnimationContext()
        {
        }

        internal AnimationContext(AnimationContextRegistry registry, object owner)
        {
            _registry = registry;
            Owner = owner;
        }

        public void ScheduleAnimation(FixedDurationAnimation animation)
        {
            ThrowIfDisposed();
            _fixedDurationAnimations.Add(animation);
        }
        
        public void ScheduleContinuingAnimation(ContinuingAnimation animation)
        {
            ThrowIfDisposed();
            _continuingAnimations.Add(animation);
        }

        public AnimationSlot CreateSlot(string? debugName = null)
        {
            ThrowIfDisposed();
            var slot = new AnimationSlot(this, debugName);
            _slots.Add(slot);
            return slot;
        }

        public void StaggerAnimations(Queue<Func<FixedDurationAnimation>> animationFactories, int frameDelay,
            Action? onCompleted = null)
        {
            ThrowIfDisposed();
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
            ThrowIfDisposed();
            if (_lastUpdatedFrame == currentFrame)
            {
                return;
            }

            _lastUpdatedFrame = currentFrame;
            UpdateFixedDurationAnimations(currentFrame);
            UpdateContinuingAnimations(currentFrame);
        }

        public void Dispose()
        {
            if (_disposed) { return; }
            ForceFinishAll();
            _fixedDurationAnimations.Clear();
            _continuingAnimations.Clear();
            _slots.Clear();
            _disposed = true;
            _registry?.Unregister(this);
        }

        public void ForceFinishAll()
        {
            ThrowIfDisposed();

            foreach (var slot in _slots.ToArray())
            {
                try
                {
                    slot.FinishNow();
                }
                catch
                {
                    // Disposal and force-finish paths should keep trying to finish the rest.
                }
            }

            ForceFinishAnimations(_continuingAnimations);
            ForceFinishAnimations(_fixedDurationAnimations);
            _continuingAnimations.RemoveAll(a => a.Completed);
            _fixedDurationAnimations.RemoveAll(a => a.Completed);
        }

        internal void RemoveAnimation(FixedDurationAnimation animation)
        {
            _fixedDurationAnimations.Remove(animation);
        }

        internal void RemoveAnimation(ContinuingAnimation animation)
        {
            _continuingAnimations.Remove(animation);
        }

        internal void RemoveSlot(AnimationSlot slot)
        {
            _slots.Remove(slot);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AnimationContext));
            }
        }

        private static void ForceFinishAnimations<TAnimation>(List<TAnimation> animations)
            where TAnimation : class
        {
            var forceFinishCount = 0;
            while (animations.Any(a => !IsCompleted(a)))
            {
                foreach (var animation in animations.Where(a => !IsCompleted(a)).ToArray())
                {
                    try
                    {
                        ForceFinish(animation);
                    }
                    catch
                    {
                        // Keep force-finishing the rest even if one animation's completion code fails.
                    }
                }

                forceFinishCount += 1;
                if (forceFinishCount > 10000)
                {
                    return;
                }
            }
        }

        private static bool IsCompleted<TAnimation>(TAnimation animation)
            where TAnimation : class
        {
            return animation switch
            {
                FixedDurationAnimation fixedDurationAnimation => fixedDurationAnimation.Completed,
                ContinuingAnimation continuingAnimation => continuingAnimation.Completed,
                _ => true
            };
        }

        private static void ForceFinish<TAnimation>(TAnimation animation)
            where TAnimation : class
        {
            switch (animation)
            {
                case FixedDurationAnimation fixedDurationAnimation:
                    fixedDurationAnimation.ForceFinish();
                    break;
                case ContinuingAnimation continuingAnimation:
                    continuingAnimation.ForceFinish();
                    break;
            }
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
