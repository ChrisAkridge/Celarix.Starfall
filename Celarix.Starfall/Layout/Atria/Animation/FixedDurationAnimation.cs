using Celarix.Starfall.Rendering.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Animation
{
    public class FixedDurationAnimation
    {
        private readonly Action<double> _updateAction;
        private readonly Action? _onCompleted;
        private readonly Action<Exception?>? _onError;

        public int StartFrame { get; private set; }
        public int Duration { get; private set; }
        public bool Completed { get; private set; }

        public int EndFrame
        {
            get => (StartFrame + Duration) - 1;
            private set
            {
                // Let us set it so ForceEnd works properly.
                if (value < StartFrame)
                {
                    throw new ArgumentException("EndFrame cannot be less than StartFrame.");
                }
                var newDuration = (value - StartFrame) + 1;
                Duration = newDuration;
            }
        }

        public FixedDurationAnimation(int startFrame, int duration, Action<double> updateAction,
            Action? onCompleted = null,
            Action<Exception?>? onError = null)
        {
            StartFrame = startFrame;
            Duration = duration;
            _updateAction = updateAction;
            
            // Kinda hackish, but ensure that we always do a P = 1.00 step at the very end to reach the final state.
            if (onCompleted == null)
            {
                _onCompleted = () => updateAction(1d);
            }
            else
            {
                _onCompleted = () =>
                {
                    updateAction(1d);
                    onCompleted();
                };
            }

            _onError = onError;
        }

        public static FixedDurationAnimation StartNow(int duration, Action<double> updateAction,
            Action? onCompleted = null,
            Action<Exception?>? onError = null)
        {
            return new FixedDurationAnimation(AtriaLayoutEngine.GlobalFrameNumber, duration, updateAction, onCompleted, onError);
        }

        public static FixedDurationAnimation StartIn(int framesFromNow, int duration, Action<double> updateAction,
            Action? onCompleted = null,
            Action<Exception?>? onError = null)
        {
            return new FixedDurationAnimation(AtriaLayoutEngine.GlobalFrameNumber + framesFromNow, duration, updateAction, onCompleted, onError);
        }

        public void Update(int currentFrame)
        {
            if (currentFrame < StartFrame || currentFrame > EndFrame)
            {
                return;
            }

            double progress = (double)(currentFrame - StartFrame) / Duration;

            try
            {
                _updateAction(progress);
            }
            catch (Exception ex)
            {
                _onError?.Invoke(ex);
                Completed = true;
            }

            if (currentFrame == EndFrame)
            {
                _onCompleted?.Invoke();
                Completed = true;
            }
        }

        public void ForceEnd(int currentFrame)
        {
            if (currentFrame < StartFrame)
            {
                StartFrame = currentFrame;
            }
            EndFrame = currentFrame;
        }
    }
}
