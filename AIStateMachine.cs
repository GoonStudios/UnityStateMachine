using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Goon.AI
{
    public class AIStateMachine : MonoBehaviour
    {
        #if UNITY_EDITOR
        [ContextMenu("Launch Editor")]
        private void LaunchEditor()
        {
            StateMachineEditor.LaunchEditor(this);
        }

        [HideInInspector]
        public bool compressed = false;
        #endif

        public List<AIState> states;
        public AIState initialState;

        private AIState currentState;

        private Queue<AIAction> actionQueue;
        private AIAction currentAction = null;

        private void Awake()
        {
            currentState = initialState;
            actionQueue = new Queue<AIAction>();
        }

        private void Update()
        {
            UpdateState();
            PerformActions();
        }

        private void UpdateState()
        {
            AITransition triggered = null;

            foreach (AITransition trans in currentState.transitions)
            {
                if (trans.IsTriggered())
                {
                    triggered = trans;
                    break;
                }
            }

            if (triggered != null)
            {
                AIState targetState = triggered.targetState;

                if (currentState.exitAction != null)
                {
                    actionQueue.Enqueue(currentState.exitAction);
                }
                if (triggered.action != null)
                {
                    actionQueue.Enqueue(triggered.action);
                }
                if (targetState.entryAction != null)
                {
                    actionQueue.Enqueue(targetState.entryAction);
                }

                currentState = targetState;
            }
            else if (currentState.action != null && actionQueue.Count == 0)
            {
                actionQueue.Enqueue(currentState.action);
            }
        }

        private void PerformActions()
        {
            if (currentAction == null)
            {
                if (actionQueue.Count > 0)
                {
                    currentAction = actionQueue.Dequeue();
                }
                else
                {
                    return;
                }
            }

            if (!currentAction.IsComplete())
            {
                currentAction.Perform();
            }

            if (currentAction.IsComplete())
            {
                currentAction.Reset();
                currentAction = null;
                PerformActions();
            }
        }
    }
}