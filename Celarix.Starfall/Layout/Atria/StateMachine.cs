using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria
{
    public sealed class StateMachine<TState> where TState : Enum
    {
        private sealed class Transition
        {
            public TState FromState { get; }
            public TState ToState { get; }
            public Action<AtriaSlide> Action { get; }
            public Transition(TState fromState, TState toState, Action<AtriaSlide> action)
            {
                FromState = fromState;
                ToState = toState;
                Action = action;
            }
        }

        private TState _currentState;
        private readonly List<Transition> _transitions = new();

        public StateMachine(AtriaSlide slide, TState currentState)
        {
            _currentState = currentState;

            // Use reflection to find methods with the StateTransitionAttribute in the slide
            var methods = slide.GetType().GetMethods(System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance);
            foreach (var method in methods)
            {
                if (method.GetCustomAttributes(typeof(StateTransitionAttribute<TState>), false)
                    .FirstOrDefault() is StateTransitionAttribute<TState> attribute)
                {
                    // Create a delegate for the method and add it to the transitions list
                    var action = (Action<AtriaSlide>)Delegate.CreateDelegate(typeof(Action<AtriaSlide>), slide, method);
                    _transitions.Add(new Transition(attribute.FromState, attribute.ToState, action));
                }
            }
        }
    }
}
