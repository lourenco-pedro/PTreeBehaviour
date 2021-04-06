using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PTreeBehaviour
{
    [Serializable]
    public class EditorInfo
    {
        public Rect WindowRect;
        public Vector2 NodePosition;
        public Vector2 NodeSize;
        public Texture2D HeaderTexture;

        public Vector2 InputPosition
        {
            get 
            {
                Vector2 toReturn = new Vector2(WindowRect.x + 5, WindowRect.y + 30);
                return toReturn;
            }
        }

        public EditorInfo() 
        {
            NodeSize = Vector2.one * 100;
            NodeSize.x += 150;
            HeaderTexture = new Texture2D(1,1);
        }
    }

    public abstract class BehaviourNode
    {

        public string NodeID => _nodeId;
        public string ParentId
        {
            get => _parentId;
            set => _parentId = value;
        }
        public BehaviourNodeType BehaviourNodeType => _behaviourNodeType;
        
        [SerializeField]
        private string _nodeId;
        [SerializeField]
        private string _parentId;
        [SerializeField]
        private BehaviourNodeType _behaviourNodeType;

        public EditorInfo InEditorWindow;

        public BehaviourNode(BehaviourNodeType type) 
        {
            _nodeId = Guid.NewGuid().ToString();
            _behaviourNodeType = type;

            #region UNITY_EDITOR
            InEditorWindow = new EditorInfo();
            #endregion
        }

        public abstract object Execute(PTreeBehaviourComponent actor);

        public virtual bool IsValidOutput(string outputId) 
        {
            bool valid = (outputId != NodeID && (!string.IsNullOrEmpty(ParentId) && outputId != ParentId || string.IsNullOrEmpty(ParentId)));
            if (!valid) 
            {
                Debug.Log("Output is not valid !");
            }
            return valid;
        }
    }
}