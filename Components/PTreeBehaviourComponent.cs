using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PTreeBehaviour
{
    public class PTreeBehaviourComponent : MonoBehaviour
    {
        public PTreeBehaviour Behaviour => _behaviour;
        public bool Enabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public BehaviourNode CurrentNode
        {
            get
            {
                if (string.IsNullOrEmpty(_runningNode))
                {
                    return null;
                }

                if (IsRoot(_runningNode)) 
                {
                    return Behaviour.RootNode;
                }

                return Behaviour.GetBaseNode(_runningNode);
            }
        }

        public Dictionary<string, RuntimeNodeState> RuntimeNodes => _runtimeNodes;

        public PTreeActor Actor;
        public PTreeTargeting Target;

        [Space]

        [SerializeField]
        private PTreeBehaviour _behaviour;
        [SerializeField]
        private bool _enabled = true;

        private string _runningNode;
        private string _toNextNode;
        private Dictionary<string, RuntimeNodeState> _runtimeNodes;

        public void Run()
        {
            if (!_enabled)
            {
                return;
            }

            if (string.IsNullOrEmpty(_toNextNode))
            {
                _toNextNode = Behaviour.RootNode.NodeID;
                SetAllNodeStates(RuntimeNodeState.NONE);
            }

            while (!string.IsNullOrEmpty(_toNextNode))
            {
                if (IsCurrentRuntimeNodeFinished())
                {
                    _runningNode = _toNextNode;
                    _toNextNode = string.Empty;
                }

                switch (CurrentNode.BehaviourNodeType)
                {
                    case BehaviourNodeType.Root:
                        {
                            RootBehaviourNode rootNode = CurrentNode as RootBehaviourNode;
                            _toNextNode = rootNode.OutputId;
                            SetCurrentRuntimeNodeState(RuntimeNodeState.FINISHED);
                        }
                        break;
                    case BehaviourNodeType.Action:
                        {
                            ActionBehaviourNode actionNode = CurrentNode as ActionBehaviourNode;
                            actionNode.Execute(this);
                            SetCurrentRuntimeNodeState(RuntimeNodeState.FINISHED);
                            _toNextNode = string.Empty;
                        }
                        break;
                    case BehaviourNodeType.Condition:
                        {
                            ConditionBehaviourNode conditionNode = CurrentNode as ConditionBehaviourNode;
                            _toNextNode = ((bool)conditionNode.Execute(this)) ? conditionNode.TrueNode : conditionNode.FalseNode;
                            SetCurrentRuntimeNodeState(RuntimeNodeState.FINISHED);
                        }
                        break;
                    case BehaviourNodeType.Sequence:
                        {
                            SequenceBehaviourNode sequenceBehaviourNode = CurrentNode as SequenceBehaviourNode;
                            foreach (var nodeId in sequenceBehaviourNode.Outputs) 
                            {
                                BehaviourNode node = Behaviour.GetBaseNode(nodeId) as ActionBehaviourNode;
                                node.Execute(this);
                            }
                            _toNextNode = string.Empty;
                        }
                        break;
                }
            }
        }

        public void SetBehaviour(PTreeBehaviour behaviour)
        {
            _behaviour = behaviour;

            _runtimeNodes = new Dictionary<string, RuntimeNodeState>();

            _runtimeNodes.Add(behaviour.RootNode.NodeID, RuntimeNodeState.NONE);

            foreach (var node in behaviour.AllNodes)
            {
                _runtimeNodes.Add(node.NodeID, RuntimeNodeState.NONE);
            }

            _runningNode = Behaviour.RootNode.NodeID;
        }

        public void SetAllNodeStates(RuntimeNodeState state, bool onlyFinishedNodes = true)
        {

            var keys = new string[_runtimeNodes.Keys.Count];
            _runtimeNodes.Keys.CopyTo(keys, 0);

            foreach (var id in keys)
            {
                if ((onlyFinishedNodes && _runtimeNodes[id] == RuntimeNodeState.RUNNING) || !onlyFinishedNodes)
                {
                    _runtimeNodes[id] = state;
                }
            }
        }

        private bool IsRoot(string nodeId)
        {
            return Behaviour.RootNode.NodeID == nodeId;
        }

        private bool IsCurrentRuntimeNodeFinished()
        {
            return _runtimeNodes[_runningNode] == RuntimeNodeState.FINISHED;
        }

        private bool IsCurrentRuntimeNodeState(RuntimeNodeState state)
        {
            return _runtimeNodes[_runningNode] == state;
        }

        private void SetCurrentRuntimeNodeState(RuntimeNodeState state) 
        {
            _runtimeNodes[_runningNode] = state;
        }
    }
}