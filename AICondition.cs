using UnityEngine;

namespace Goon.AI
{
    public abstract class AICondition : MonoBehaviour
    {
        public abstract bool IsTriggered();
    }
}