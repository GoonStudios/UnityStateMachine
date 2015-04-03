using UnityEngine;
using System.Collections;

namespace Goon.AI.Test
{
    public class TestAction : AIAction
    {
        public string actionText = "";
        private bool complete = false;

        public override void Perform()
        {
            Debug.Log(actionText);
            complete = true;
        }

        public override bool isComplete()
        {
            return complete;
        }

        public override void Reset()
        {
            complete = false;
        }
    }
}
