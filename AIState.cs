using UnityEngine;
using System.Collections.Generic;

namespace Goon.AI
{
    public class AIState : MonoBehaviour
    {
        #if UNITY_EDITOR
        public string stateName = "State";
        public Vector2 nodePos = Vector2.zero;
        #endif

        [Space(10)]
        public AIAction action;
        public AIAction entryAction;
        public AIAction exitAction;

        public List<AITransition> transitions;
    }
}