using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout
{
    public interface ILayoutEngine<TScene, TTransition>
        where TTransition : ITransition
    {
        void Render(TScene scene);
        void Transition(TScene from, TScene to, TTransition transition);

        /// <summary>
        /// Gets a value indicating whether the current scene shown by the layout engine is animating.
        /// This has nothing to do with transitions, which are handled by PresentationEngine. This is
        /// for scenes that have their own internal animation, either finite or infinite. If a finitely
        /// animating scene completes its animation, this property will return <see cref="false"/>.
        /// </summary>
        bool IsAnimating { get; }
    }
}
