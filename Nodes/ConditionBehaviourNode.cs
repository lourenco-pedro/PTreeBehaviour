using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PTreeBehaviour
{
    [Serializable]
    public class ConditionBehaviourNode : BehaviourNode
    {

        public ConditionNodeParameter ConditionNodeParameter
        {
            get => _conditionNodeParameter;
            set => _conditionNodeParameter = value;
        }
        public string FalseNode => _falseChildId;
        public string TrueNode => _trueChildId;

        [SerializeField]    
        private ConditionNodeParameter _conditionNodeParameter;
        [SerializeField]    
        private string _falseChildId;
        [SerializeField]    
        private string _trueChildId;

        public ConditionBehaviourNode(BehaviourNodeType type = BehaviourNodeType.Condition) : base(type)
        {
            _falseChildId = string.Empty;
            _trueChildId = string.Empty;
        }

        public override object Execute(PTreeBehaviourComponent actor)
        {
            return _conditionNodeParameter == null ? false : _conditionNodeParameter.Run(actor);
        }

        public void SetTrueChildId(string toId) 
        {
            if (!IsValidOutput(toId))
            {
                return;
            }

            _trueChildId = toId;
        }
        
        public void SetFalseChildId(string toId) 
        {
            if (toId == NodeID || (!string.IsNullOrEmpty(ParentId) && toId == ParentId))
            {
                return;
            }

            _falseChildId = toId;
        }
    }
}