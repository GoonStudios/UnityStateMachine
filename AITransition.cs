using UnityEngine;

namespace Goon.AI
{
    public class AITransition : MonoBehaviour
    {
        #if UNITY_EDITOR
        public string transitionName = "Transition";
        public Vector2 nodePos = Vector2.zero;
        #endif

        public bool IsTriggered()
        {
            return condition.IsTriggered();
        }

        [Space(10)]
        public AIState targetState;
        public AICondition condition;
        public AIAction action;
    }
}