using UnityEngine;
using System.Collections;

namespace Goon.AI.Test
{
    public class TestCondition : AICondition
    {

        public bool trigger = false;

        public override bool IsTriggered()
        {
            if (trigger)
            {
                trigger = false;
                return true;
            }

            return false;
        }
    }
}
