using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace PTreeBehaviour
{
    [CreateAssetMenu(menuName = "PTreeBehaviour/new Behaviour", fileName = "NewBehaviour")]
    public class PTreeBehaviour : ScriptableObject
    {
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        [SerializeField]
        private string _name;

        public string[] AllNodeIds;
        public BehaviourNode[] AllNodes 
        {
            get 
            {
                BehaviourNode[] allNodes = new BehaviourNode[AllConditionNodes.Length + AllActionNodes.Length + AllSequenceNodes.Length];
                AllConditionNodes.CopyTo(allNodes, 0);
                AllActionNodes.CopyTo(allNodes, AllConditionNodes.Length);
                AllSequenceNodes.CopyTo(allNodes, AllConditionNodes.Length + AllActionNodes.Length);

                return allNodes;
            }
        }

        public RootBehaviourNode RootNode;
        public ConditionBehaviourNode[] AllConditionNodes;
        public ActionBehaviourNode[] AllActionNodes;
        public SequenceBehaviourNode[] AllSequenceNodes;

        public BehaviourNode GetBaseNode(string id) 
        {
            var allNodes = AllNodes;
            foreach (var node in allNodes) 
            {
                if (node.NodeID == id) 
                {
                    return node;
                }
            }

            return null;
        }

        public NodeType GetNode<NodeType>(Type type, string id)
            where NodeType : BehaviourNode
        {

            foreach (var node in AllNodes) 
            {
                if (node.NodeID == id) 
                {
                    return node as NodeType;
                }
            }

            return null;
        }

        public void RemoveNode<NodeType>(Type type, NodeType node)
            where NodeType : BehaviourNode
        {

            ClearParentsFromChildren(node);

            AllNodeIds = AllNodeIds.Where(id => id != node.NodeID).ToArray();
            if (type == typeof(ConditionBehaviourNode))
            {
                AllConditionNodes = AllConditionNodes.Where(targetNode => targetNode != node).ToArray();
            }
            else if (type == typeof(ActionBehaviourNode))
            {
                AllActionNodes = AllActionNodes.Where(targetNode => targetNode != node).ToArray();
            }
            else if (type == typeof(SequenceBehaviourNode)) 
            {
                AllSequenceNodes = AllSequenceNodes.Where(targetNode => targetNode != node).ToArray();
            }
        }

        public NodeType CreateNode<NodeType>(Type type)
            where NodeType : BehaviourNode
        {

            NodeType node = null;

            if (type == typeof(ConditionBehaviourNode))
            {
                node = CreateConditionBehaviourNode() as NodeType;
            }
            else if (type == typeof(ActionBehaviourNode))
            {
                node = CreateActionBehaviourNode() as NodeType;
            }
            else if (type == typeof(SequenceBehaviourNode)) 
            {
                node = CreateSequenceBehaviourNode() as NodeType;
            }

            Array.Resize(ref AllNodeIds, AllNodeIds.Length + 1);
            AllNodeIds[AllNodeIds.Length - 1] = node.NodeID;

            return node;
        }

        private void ClearParentsFromChildren<NodeType>(NodeType node)
            where NodeType : BehaviourNode
        {
            if (node is ConditionBehaviourNode)
            {
                ConditionBehaviourNode conditionBehaviourName = node as ConditionBehaviourNode;
                BehaviourNode falseNode = GetBaseNode(conditionBehaviourName.FalseNode);
                if (falseNode != null)
                {
                    falseNode.ParentId = string.Empty;
                }
                BehaviourNode trueNode = GetBaseNode(conditionBehaviourName.TrueNode);
                if (trueNode != null)
                {
                    trueNode.ParentId = string.Empty;
                }
            }
            else if (node is ActionBehaviourNode)
            {
                ActionBehaviourNode actionBehaviourNode = node as ActionBehaviourNode;
                BehaviourNode childNode = GetBaseNode(actionBehaviourNode.OutputId);
                if (childNode != null)
                {
                    childNode.ParentId = string.Empty;
                }
            }
            else if (node is SequenceBehaviourNode) 
            {
                SequenceBehaviourNode sequenceBehaviourNode = node as SequenceBehaviourNode;
                foreach (string child in sequenceBehaviourNode.Outputs) 
                {
                    if (string.IsNullOrEmpty(child)) 
                    {
                        continue;
                    }

                    BehaviourNode childNode = GetBaseNode(child);
                    if (childNode != null) 
                    {
                        childNode.ParentId = string.Empty;
                    }
                }
            }
        }

        private ConditionBehaviourNode CreateConditionBehaviourNode() 
        {
            ConditionBehaviourNode node = new ConditionBehaviourNode();
            Array.Resize(ref AllConditionNodes, AllConditionNodes.Length + 1);
            AllConditionNodes[AllConditionNodes.Length - 1] = node;
            return node;
        }

        private ActionBehaviourNode CreateActionBehaviourNode() 
        {
            ActionBehaviourNode node = new ActionBehaviourNode();
            Array.Resize(ref AllActionNodes, AllActionNodes.Length + 1);
            AllActionNodes[AllActionNodes.Length - 1] = node;
            return node;
        }

        private SequenceBehaviourNode CreateSequenceBehaviourNode() 
        {
            SequenceBehaviourNode node = new SequenceBehaviourNode();
            Array.Resize(ref AllSequenceNodes, AllSequenceNodes.Length + 1);
            AllSequenceNodes[AllSequenceNodes.Length - 1] = node;
            return node;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (RootNode == null || string.IsNullOrEmpty(RootNode.NodeID)) 
            {
                RootNode = new RootBehaviourNode();
                var lastAllNodes = (string[])AllNodeIds.Clone();
                Array.Resize(ref AllNodeIds, AllNodeIds.Length + 1);
                AllNodeIds[0] = RootNode.NodeID;
                lastAllNodes.CopyTo(AllNodeIds, 1);
            }

            if (AllNodeIds == null) 
            {
                AllNodeIds = new string[0];
            }

            if (AllActionNodes == null) 
            {
                AllActionNodes = new ActionBehaviourNode[0];
            }

            if (AllConditionNodes == null) 
            {
                AllConditionNodes = new ConditionBehaviourNode[0];
            }

            if (AllSequenceNodes == null) 
            {
                AllSequenceNodes = new SequenceBehaviourNode[0];
            }

            if (string.IsNullOrEmpty(_name)) 
            {
                _name = "New Behaviour Tree";
            }
        }
#endif
    }
}