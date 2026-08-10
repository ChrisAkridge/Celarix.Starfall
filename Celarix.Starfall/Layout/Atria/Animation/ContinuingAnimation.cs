using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Animation
{
    public class ContinuingAnimation
    {
        private const int MaxForceFinishIterations = 10000;
        private readonly Func<bool> _updateAction;
        private readonly Action<Exception?>? _onError;

        public int StartFrame { get; private set; }
        public bool Completed { get; private set; }

        public ContinuingAnimation(int startFrame, Func<bool> updateAction, Action<Exception?>? onError = null)
        {
            StartFrame = startFrame;
            _updateAction = updateAction;
            _onError = onError;
        }

        public static ContinuingAnimation StartNow(Func<bool> updateAction, Action<Exception?>? onError = null)
        {
            return new ContinuingAnimation(AtriaLayoutEngine.GlobalFrameNumber, updateAction, onError);
        }

        public static ContinuingAnimation StartIn(int framesFromNow, Func<bool> updateAction, Action<Exception?>? onError = null)
        {
            return new ContinuingAnimation(AtriaLayoutEngine.GlobalFrameNumber + framesFromNow, updateAction, onError);
        }

        public void Update(int currentFrame)
        {
            if (currentFrame < StartFrame || Completed)
            {
                return;
            }

            try
            {
                var shouldContinue = _updateAction();
                if (!shouldContinue)
                {
                    Completed = true;
                }
            }
            catch (Exception ex)
            {
                _onError?.Invoke(ex);
                Completed = true;
            }
        }

        public virtual void ForceFinish()
        {
            if (Completed) { return; }

            try
            {
                var iterations = 0;
                while (!Completed)
                {
                    var shouldContinue = _updateAction();
                    if (!shouldContinue)
                    {
                        Completed = true;
                        return;
                    }

                    iterations += 1;
                    if (iterations >= MaxForceFinishIterations)
                    {
                        Completed = true;
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _onError?.Invoke(ex);
                Completed = true;
            }
        }
    }
}
