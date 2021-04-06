using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PTreeBehaviour
{
    [Serializable]
    public class ActionBehaviourNode : BehaviourNode
    {
        public string OutputId => _outputId;

        public ActionNodeParameter ActionNodeParameter
        {
            get => _actionNodeParameter;
            set => _actionNodeParameter = value;
        }

        [SerializeField]
        private string _outputId;

        [SerializeField]
        private ActionNodeParameter _actionNodeParameter;

        public ActionBehaviourNode(BehaviourNodeType type = BehaviourNodeType.Action) : base(type) 
        {
            _outputId = string.Empty;
        }

        public void SetOutputChild(string toChild) 
        {
            if (!IsValidOutput(toChild)) 
            {
                return;
            }

            _outputId = toChild;
        }

        public override object Execute(PTreeBehaviourComponent actor)
        {
            _actionNodeParameter.Run(actor);
            return 0;
        }
    }
}