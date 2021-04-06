using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace PTreeBehaviour
{
    [Serializable]
    public class SequenceBehaviourNode : BehaviourNode
    {

        public string[] Outputs => _outputs;
        
        [SerializeField]
        private string[] _outputs;

        public SequenceBehaviourNode(BehaviourNodeType type = BehaviourNodeType.Sequence) : base(type) 
        {
            _outputs = new string[0];
            AddSequenceOutput();
        }

        public override object Execute(PTreeBehaviourComponent actor)
        {
            return null;
        }

        public void SetChild(int index, string child) 
        {
            if (!IsValidOutput(child)) 
            {
                return;
            }

            _outputs[index] = child;
        }

        public void AddSequenceOutput(Action onOutputAdded = null) 
        {
            Array.Resize(ref _outputs, _outputs.Length + 1);
            _outputs[_outputs.Length - 1] = string.Empty;
            onOutputAdded?.Invoke();
        }

        public void RemoveSequenceOutput(Action onOutputRemoved = null) 
        {
            _outputs = _outputs.Where((item, index) => index != _outputs.Length - 1).ToArray();
            onOutputRemoved?.Invoke();
        }

        public void ClearOutputs(Action onOutputCleared = null) 
        {
            _outputs = new string[0];
            onOutputCleared?.Invoke();
        }
    }
}