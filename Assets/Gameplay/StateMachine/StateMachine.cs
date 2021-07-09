    using UnityEngine;

    namespace StateMachineLogic
    {
        public class StateMachine<T>
        {
            public State<T> currentState { get; private set; }
            public T owner;

            public StateMachine(T _o)
            {
                owner = _o;
                currentState = null;
            }

            public void ChangeState(State<T> _newstate, byte stateEnumRef = default)
            {
                if (currentState != null)
                    currentState.ExitState();

                currentState = _newstate;

                currentState.Owner = owner;
                currentState.EnterState();
            }

            public void Update()
            {
                if (currentState != null)
                    currentState.UpdateState();
            }
        }

        public abstract class State<T>
        {
            public T Owner;

            public abstract void EnterState();

            public abstract void ExitState();

            public abstract void UpdateState();
        }
    }