using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PTreeBehaviour
{
    [Serializable]
    public class RootBehaviourNode : BehaviourNode
    {

        public string OutputId => _outputId;

        [SerializeField]
        private string _outputId;

        public RootBehaviourNode(BehaviourNodeType type = BehaviourNodeType.Root) : base(type) 
        {
        }

        public override object Execute(PTreeBehaviourComponent actor)
        {
            return null;
        }

        public void SetOutputChildId(string output) 
        {
            if (!IsValidOutput(output)) 
            {
                return;
            }

            _outputId = output;
        }
    }
}