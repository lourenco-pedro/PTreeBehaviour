using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PTreeBehaviour
{
    public enum BehaviourNodeType 
    {
        Condition,
        Sequence,
        Action,
        Root
    }

    public enum RuntimeNodeState 
    {
        NONE,
        RUNNING,
        FINISHED
    }
}
