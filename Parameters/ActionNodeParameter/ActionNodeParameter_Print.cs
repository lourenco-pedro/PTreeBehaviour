using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PTreeBehaviour
{
    [CreateAssetMenu(menuName = "PTreeBehaviour/NodeParameter/Action/Print", fileName = "NewActionNode")]
    public class ActionNodeParameter_Print : ActionNodeParameter
    {
        [SerializeField]
        private string _message;
        
        public override void Run(PTreeBehaviourComponent actor)
        {
        }
    }
}