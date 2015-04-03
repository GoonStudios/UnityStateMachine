using UnityEngine;
using System;

namespace Goon.AI
{
    public abstract class AIAction : MonoBehaviour
    {
        public abstract void Perform();
        public abstract bool isComplete();
        public abstract void Reset();
    }
}