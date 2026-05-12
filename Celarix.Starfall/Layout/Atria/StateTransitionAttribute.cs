using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class StateTransitionAttribute<TState> : Attribute where TState : Enum
    {
        public TState FromState { get; }
        public TState ToState { get; }

        public StateTransitionAttribute(TState fromState, TState toState)
        {
            FromState = fromState;
            ToState = toState;
        }
    }
}
