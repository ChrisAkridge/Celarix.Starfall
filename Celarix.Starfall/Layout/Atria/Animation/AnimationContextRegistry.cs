using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Celarix.Starfall.Layout.Atria.Animation
{
    public sealed class AnimationContextRegistry
    {
        private readonly List<AnimationContext> _contexts = [];

        public bool IsAnythingAnimating => _contexts.Any(c => c.IsAnimating);
        public int RunningAnimationCount => _contexts.Sum(c => c.RunningAnimationCount);
        public IReadOnlyList<AnimationContext> Contexts => _contexts;

        public AnimationContext CreateFor(object owner)
        {
            var context = new AnimationContext(this, owner);
            _contexts.Add(context);
            return context;
        }

        public void DisposeOwnedBy(object owner)
        {
            foreach (var context in _contexts.Where(c => ReferenceEquals(c.Owner, owner)).ToArray())
            {
                context.Dispose();
            }
        }

        public void UpdateAll(int currentFrame)
        {
            foreach (var context in _contexts.ToArray())
            {
                context.Update(currentFrame);
            }
        }

        internal void Unregister(AnimationContext context)
        {
            _contexts.Remove(context);
        }
    }
}
