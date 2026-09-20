using System;

namespace Celarix.Starfall.Layout.Atria.Animation
{
    public sealed class AnimationSlot : IDisposable
    {
        private readonly AnimationContext _context;
        private FixedDurationAnimation? _fixedDurationAnimation;
        private ContinuingAnimation? _continuingAnimation;
        private bool _disposed;

        internal AnimationSlot(AnimationContext context, string? debugName)
        {
            _context = context;
            DebugName = debugName;
        }

        public string? DebugName { get; }
        public bool IsRunning => (_fixedDurationAnimation?.Completed == false)
            || (_continuingAnimation?.Completed == false);

        public void Replace(FixedDurationAnimation animation,
            AnimationSlotReplacementBehavior replacementBehavior = AnimationSlotReplacementBehavior.ForceFinishExisting)
        {
            ThrowIfDisposed();
            ReplaceCurrentAnimation(replacementBehavior);
            _fixedDurationAnimation = animation;
            _context.ScheduleAnimation(animation);
        }

        public void Replace(Func<FixedDurationAnimation> animationFactory,
            AnimationSlotReplacementBehavior replacementBehavior = AnimationSlotReplacementBehavior.ForceFinishExisting)
        {
            ThrowIfDisposed();
            ReplaceCurrentAnimation(replacementBehavior);
            _fixedDurationAnimation = animationFactory();
            _context.ScheduleAnimation(_fixedDurationAnimation);
        }

        public void Replace(ContinuingAnimation animation,
            AnimationSlotReplacementBehavior replacementBehavior = AnimationSlotReplacementBehavior.ForceFinishExisting)
        {
            ThrowIfDisposed();
            ReplaceCurrentAnimation(replacementBehavior);
            _continuingAnimation = animation;
            _context.ScheduleContinuingAnimation(animation);
        }

        public void Replace(Func<ContinuingAnimation> animationFactory,
            AnimationSlotReplacementBehavior replacementBehavior = AnimationSlotReplacementBehavior.ForceFinishExisting)
        {
            ThrowIfDisposed();
            ReplaceCurrentAnimation(replacementBehavior);
            _continuingAnimation = animationFactory();
            _context.ScheduleContinuingAnimation(_continuingAnimation);
        }

        public void Cancel()
        {
            ThrowIfDisposed();
            CancelCurrentAnimation();
        }

        public void FinishNow()
        {
            ThrowIfDisposed();
            ForceFinishCurrentAnimation();
        }

        public void Dispose()
        {
            if (_disposed) { return; }
            ForceFinishCurrentAnimation();
            _context.RemoveSlot(this);
            _disposed = true;
        }

        private void ReplaceCurrentAnimation(AnimationSlotReplacementBehavior replacementBehavior)
        {
            switch (replacementBehavior)
            {
                case AnimationSlotReplacementBehavior.ForceFinishExisting:
                    ForceFinishCurrentAnimation();
                    break;
                case AnimationSlotReplacementBehavior.CancelExisting:
                    CancelCurrentAnimation();
                    break;
                case AnimationSlotReplacementBehavior.LeaveExistingRunning:
                    ClearCurrentAnimationReferences();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(replacementBehavior), replacementBehavior, null);
            }
        }

        private void ForceFinishCurrentAnimation()
        {
            if (_fixedDurationAnimation != null)
            {
                _fixedDurationAnimation.ForceFinish();
                _context.RemoveAnimation(_fixedDurationAnimation);
            }

            if (_continuingAnimation != null)
            {
                _continuingAnimation.ForceFinish();
                _context.RemoveAnimation(_continuingAnimation);
            }

            ClearCurrentAnimationReferences();
        }

        private void CancelCurrentAnimation()
        {
            if (_fixedDurationAnimation != null)
            {
                _context.RemoveAnimation(_fixedDurationAnimation);
            }

            if (_continuingAnimation != null)
            {
                _context.RemoveAnimation(_continuingAnimation);
            }

            ClearCurrentAnimationReferences();
        }

        private void ClearCurrentAnimationReferences()
        {
            _fixedDurationAnimation = null;
            _continuingAnimation = null;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AnimationSlot));
            }
        }
    }
}
