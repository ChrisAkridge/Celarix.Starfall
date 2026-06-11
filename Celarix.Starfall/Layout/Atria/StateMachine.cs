using Celarix.Starfall.Rendering.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.Starfall.Layout.Atria
{
    public class StateMachine<TState> where TState : Enum
    {
        protected sealed class Transition
        {
            public TState FromState { get; }
            public TState ToState { get; }
            public Action Action { get; }
            public Transition(TState fromState, TState toState, Action action)
            {
                FromState = fromState;
                ToState = toState;
                Action = action;
            }
        }

        private AtriaSlide _slide;
        protected TState _currentState;
        protected readonly List<Transition> _transitions = new();

        public TState CurrentState => _currentState;

        public StateMachine(AtriaSlide slide, TState currentState)
        {
            _slide = slide;
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
                    var action = (Action)Delegate.CreateDelegate(typeof(Action), slide, method);
                    _transitions.Add(new Transition(attribute.FromState, attribute.ToState, action));
                }
            }
        }

        public void GoToState(TState newState)
        {
            var transition = _transitions.FirstOrDefault(t => t.FromState.Equals(_currentState) && t.ToState.Equals(newState));
            if (transition != null)
            {
                transition.Action.Invoke();
                _currentState = newState;
            }
            else
            {
                throw new InvalidOperationException($"No transition defined from state {_currentState} to state {newState}.");
            }
        }
    }

    /// <summary>
    /// A state machine for simple slides that only need to transition forwards or backwards.
    /// </summary>
    /// <typeparam name="TState">The type of the state enumeration.</typeparam>
    public class LinearStateMachine<TState> : StateMachine<TState> where TState : Enum
    {
        public LinearStateMachine(AtriaSlide slide, TState initialState) : base(slide, initialState)
        {
        }

        public void Advance()
        {

        }
    }
}