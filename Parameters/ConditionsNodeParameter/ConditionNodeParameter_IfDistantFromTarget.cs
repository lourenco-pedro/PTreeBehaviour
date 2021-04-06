using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PTreeBehaviour
{
    [CreateAssetMenu(menuName = "PTreeBehaviour/NodeParameter/Condition/If Distant from target", fileName = "NewConditionNode")]
    public class ConditionNodeParameter_IfDistantFromTarget : ConditionNodeParameter
    {   
        public float MaxDistance => _maxDistance;

        [SerializeField]
        private float _maxDistance = 1f;

        public override bool Run(PTreeBehaviourComponent actor)
        {
            return 1 + 1 == 2;
        }
    }
}