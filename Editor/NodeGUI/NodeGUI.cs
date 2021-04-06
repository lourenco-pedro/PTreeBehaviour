using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace PTreeBehaviour.Editor
{
    public class NodeGUI<NodeType>
        where NodeType : BehaviourNode
    {
        public readonly NodeType Node;
        public readonly string Title;
        public readonly Vector2 ParentConnectorPosition;

        public readonly Action OnClick;

        public List<NodeGUIConnector> OutputsConnectors;

        private PTreeBehaviourNodeEditor.EditorData _editorData;

        private float _nodeMinHeight = 100;
        private float _nodeHeightOfset;

        private Texture2D _texture2D_header;
        private Texture2D _texture2D_connectorParentColor;

        #region - Draw - 

        public NodeGUI(PTreeBehaviourNodeEditor.EditorData editorData, NodeType node, string title, Action onClick = null) 
        {
            Node = node;
            Title = title;
            
            OnClick = onClick;

            _editorData = editorData;

            ParentConnectorPosition = new Vector2(0, node.InEditorWindow.NodeSize.y / 2);

            _texture2D_header = new Texture2D(1,1);
            _texture2D_connectorParentColor = new Texture2D(1,1);

            _texture2D_connectorParentColor.SetPixel(0,0, new Color(91f / 255f, 215f / 255f, 217f / 255f));
            _texture2D_connectorParentColor.Apply();

            OutputsConnectors = new List<NodeGUIConnector>();

            switch (Node.BehaviourNodeType) 
            {
                case BehaviourNodeType.Root: 
                    {
                        NodeGUIConnector output = new NodeGUIConnector("Begin", (id) => { OnConnectorClicked(id); });
                        OutputsConnectors.Add(output);
                    }
                    break;
                case BehaviourNodeType.Condition:
                    {
                        _texture2D_header.SetPixel(0, 0, new Color(252f / 255f, 192f / 255f, 88 / 255f));
                        _texture2D_header.Apply();

                        NodeGUIConnector falseConnector = new NodeGUIConnector("False", (id) => { OnConnectorClicked(id); });
                        NodeGUIConnector trueConnector = new NodeGUIConnector("True", (id) => { OnConnectorClicked(id); });

                        OutputsConnectors.Add(falseConnector);
                        OutputsConnectors.Add(trueConnector);
                    }
                    break;
                case BehaviourNodeType.Action:
                    {
                        _texture2D_header.SetPixel(0, 0, new Color(43f / 255f, 214f / 255f, 252f / 255f));
                        _texture2D_header.Apply();

                        NodeGUIConnector output = new NodeGUIConnector("Output", (id) => { OnConnectorClicked(id); });
                    }
                    break;
                case BehaviourNodeType.Sequence:
                    {
                        _texture2D_header.SetPixel(0, 0, new Color(0f / 255f, 155f / 255f, 0f / 255f));
                        _texture2D_header.Apply();

                        SequenceBehaviourNode sequence = Node as SequenceBehaviourNode;

                        int i = 0;

                        foreach (var connector in sequence.Outputs) 
                        {
                            OutputsConnectors.Add( new NodeGUIConnector($"#{i}", (id) => { OnConnectorClicked(id); }) );
                            i++;
                        }
                    }
                    break;
            }
        }

        public Rect Draw(int id)
        {
            if (Node.InEditorWindow.WindowRect == Rect.zero) 
            {
                Node.InEditorWindow.WindowRect = new Rect(Node.InEditorWindow.NodePosition, Node.InEditorWindow.NodeSize);
            }

            Rect rect = GUILayout.Window(id, Node.InEditorWindow.WindowRect, DrawNodeBody, "", AddPadding(new RectOffset(0, 0, -10, 0), new GUIStyle(GUI.skin.window)));
            Node.InEditorWindow.WindowRect.size = new Vector2(Node.InEditorWindow.NodeSize.x, _nodeMinHeight + _nodeHeightOfset);
            return rect;
        }

        private void DrawNodeBody(int id) 
        {
            GUILayout.BeginVertical();
            {
                NodeTitle();

                GUILayout.BeginHorizontal();
                {
                    DrawParentConnector();
                    DrawNodeContent();
                    DrawChildConnectors();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void NodeTitle() 
        {
            GUILayout.Label(Title, AddPadding(new RectOffset(5, 0, 2, 0) , AddBackground(h2(), _texture2D_header)));
        }

        private void DrawParentConnector()
        {
            if (Node.BehaviourNodeType != BehaviourNodeType.Root)
            {
                GUILayout.BeginArea(new Rect(new Vector2(0, 24), new Vector2(GetNodeWidthPercentage(.1f), Node.InEditorWindow.NodeSize.y)));
                {
                    DrawConnector(null,
                    0, new Color(91f / 255f, 215f / 255f, 217f / 255f), displayTitle: false);
                }
                GUILayout.EndArea();
            }
        }

        private void DrawNodeContent() 
        {
            GUILayout.BeginVertical(AddMargin(new RectOffset(25, 0, 0, 0), new GUIStyle()));
            {
                GUILayout.Label("", GUILayout.MaxWidth(150));

                switch (Node.BehaviourNodeType) 
                {
                    case BehaviourNodeType.Condition:
                        GUILayout.Label("Condition", GUILayout.MaxWidth(150));
                        var conditionNode = Node as ConditionBehaviourNode;
                        conditionNode.ConditionNodeParameter = (ConditionNodeParameter)EditorGUILayout.ObjectField(conditionNode.ConditionNodeParameter, typeof(ConditionNodeParameter), false, GUILayout.MaxWidth(150));
                        break;
                    case BehaviourNodeType.Action:
                        GUILayout.Label("Action", GUILayout.MaxWidth(150));
                        var actionNode = Node as ActionBehaviourNode;
                        actionNode.ActionNodeParameter = (ActionNodeParameter)EditorGUILayout.ObjectField(actionNode.ActionNodeParameter, typeof(ActionNodeParameter), false, GUILayout.MaxWidth(150));
                        break;
                    case BehaviourNodeType.Sequence:
                        break;
                } 
            }
            GUILayout.EndVertical();
        }

        private void DrawChildConnectors() 
        {
            GUILayout.BeginVertical();
            {
                int id = 0;
                foreach (var connector in OutputsConnectors)
                {
                    DrawConnector(connector, id, new Color(244f / 255f, 247f / 255f, 17f / 255f), right: false);
                    id++;
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawConnector(NodeGUIConnector connectorData, int outputId, Color connectorColor, bool displayTitle = true, bool right = true) 
        {
            GUILayout.BeginHorizontal();
            {
                if (right) 
                {
                    GUI.backgroundColor = connectorColor;
                    if (GUILayout.Button("", NodeConnectorStyle()))
                    {
                        if (connectorData != null)
                        {
                            connectorData?.OnClick?.Invoke(outputId);
                        }
                    }
                    GUI.backgroundColor = Color.white;

                    if (displayTitle)
                    {
                        GUILayout.Label(connectorData?.Name, GUILayout.Width(50));
                    }
                }
                else
                {
                    if (displayTitle)
                    {
                        GUILayout.Label(connectorData?.Name, GUILayout.Width(50));
                    }
                    
                    GUI.backgroundColor = connectorColor;
                    if (GUILayout.Button("", NodeConnectorStyle()))
                    {
                        if (connectorData != null)
                        {
                            connectorData?.OnClick?.Invoke(outputId);
                        }
                    }
                    GUI.backgroundColor = Color.white;
                }
            }
            GUILayout.EndHorizontal();
        }

        #endregion

        #region - Util - 

        float GetNodeWidthPercentage(float percentage = 1)
        {
            if (percentage < 0 || percentage > 1)
            {
                return Node.InEditorWindow.NodeSize.x;
            }

            return Node.InEditorWindow.NodeSize.x * percentage;
        }

        void OnConnectorClicked(int outputId) 
        {
            _editorData.FromCreatingNodeOutputID = outputId;
            _editorData.CurrentEvent = PTreeBehaviourNodeEditor.EditorData.InputEventType.CreatingConnection;
        }

        #endregion

        #region - Styles - 

        public GUIStyle h2()
        {
            GUIStyle h2 = new GUIStyle();
            h2.fontSize = 18;
            h2.normal.textColor = Color.white;
            return h2;
        }

        GUIStyle AddBackground(GUIStyle style, Texture2D background, float colorMultiplier = 1) 
        {
            Texture2D text = new Texture2D(1, 1);
            Color color = background.GetPixel(0,0);
            color = new Color(color.r * colorMultiplier, color.g * colorMultiplier, color.b * colorMultiplier);
            text.SetPixel(0, 0, color);
            text.Apply();
            style.normal.background = text;
            return style;
        }

        GUIStyle AddMargin(RectOffset margin, GUIStyle target)
        {
            target.margin = margin;
            return target;
        }

        GUIStyle AddPadding(RectOffset padding, GUIStyle target)
        {
            target.padding = padding;
            return target;
        }

        GUIStyle NodeConnectorStyle() 
        {
            GUIStyle style = new GUIStyle(GUI.skin.horizontalSliderThumb);
            return style;
        }

        #endregion
    }
}