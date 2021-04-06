using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PTreeBehaviour
{
    public abstract class ConditionNodeParameter : BehaviourNodeParameter
    {
        public virtual bool Run(PTreeBehaviourComponent actor)
        {
            return true;
        }
    }
}