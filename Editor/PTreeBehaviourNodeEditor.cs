using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace PTreeBehaviour.Editor
{

    public class PTreeBehaviourNodeEditor : UnityEditor.EditorWindow
    {
        public class EditorData
        {
            public enum InputEventType { None, DisplayContextMenu, CreatingConnection, CreateConnection, Drag }

            public PTreeBehaviour Behaviour;
            public Vector2 ScreenPosition;

            public InputEventType CurrentEvent;

            public string ClickedNode;
            public string FromCreatingNode;
            public int FromCreatingNodeOutputID;

            public bool DeleteNode;
            public bool ChangingTitle;
            public bool AddConnectionFromInput;
            public bool IsAddConnection => (AddConnectionFromInput == true);

            public EditorData()
            {
                ScreenPosition = Vector2.zero;
            }
        }

        EditorData data;

        [MenuItem("Window/Alyns/PTreeBehaviour")]
        public static void OpenWindow()
        {
            var window = (PTreeBehaviourNodeEditor)GetWindow(typeof(PTreeBehaviourNodeEditor));
            window.maxSize = Vector2.right * (1.8f * Screen.width) + Vector2.up * (1.5f * Screen.height);
            window.minSize = Vector2.right * (1.8f * Screen.width) + Vector2.up * (1.5f * Screen.height);
            window.data = new EditorData();
        }

        #region - Inputs - 

        void DetectEvents(Event e)
        {
            if (e == null)
                return;

            if (e.isMouse)
            {
                if (IsLeftMouseButtenEvent(e) && e.delta == Vector2.zero)
                {
                    string lastClickedNode = data.ClickedNode;
                    bool inNode = IsInNode(e.mousePosition, out data.ClickedNode);

                    if (inNode && data.CurrentEvent == EditorData.InputEventType.CreatingConnection)
                    {
                        data.FromCreatingNode = lastClickedNode;
                        data.CurrentEvent = EditorData.InputEventType.CreateConnection;
                    }
                    else
                    {
                        data.CurrentEvent = EditorData.InputEventType.None;
                    }
                    return;
                }
                else if (IsRightMouseButtonEvent(e))
                {
                    data.DeleteNode = IsInNode(e.mousePosition, out data.ClickedNode);
                    data.CurrentEvent = EditorData.InputEventType.DisplayContextMenu;
                    return;
                }
                else if (IsMiddleMouseButtonEvent(e)) 
                {
                    data.CurrentEvent = EditorData.InputEventType.Drag;
                    return;
                }
            }
            else if (e.isKey)
            {
                data.CurrentEvent = EditorData.InputEventType.None;
                return;
            }
        }

        void ManageEvents(Event e)
        {
            switch (data.CurrentEvent)
            {

                case EditorData.InputEventType.DisplayContextMenu:
                    {

                        if (data.CurrentEvent == EditorData.InputEventType.None)
                        {
                            return;
                        }
                        GenericMenu menu = new GenericMenu();

                        if (data.DeleteNode)
                        {
                            if (data.ClickedNode != data.Behaviour.RootNode.NodeID)
                            {
                                menu.AddItem(new GUIContent("Delete node"), false, () =>
                                {
                                    BehaviourNode node = data.Behaviour.GetBaseNode(data.ClickedNode);
                                    if (node.BehaviourNodeType == BehaviourNodeType.Action)
                                    {
                                        var actionNode = node as ActionBehaviourNode;
                                        data.Behaviour.RemoveNode((typeof(ActionBehaviourNode)), actionNode);
                                    }

                                    else if (node.BehaviourNodeType == BehaviourNodeType.Condition)
                                    {
                                        var conditionNode = node as ConditionBehaviourNode;
                                        data.Behaviour.RemoveNode((typeof(ConditionBehaviourNode)), conditionNode);
                                    }
                                    else if (node.BehaviourNodeType == BehaviourNodeType.Sequence)
                                    {
                                        var sequence = node as SequenceBehaviourNode;
                                        data.Behaviour.RemoveNode(typeof(SequenceBehaviourNode), sequence);
                                    }

                                    data.ClickedNode = string.Empty;
                                });
                                
                                menu.AddSeparator("");
                            }

                            BehaviourNode baseNode = data.Behaviour.GetBaseNode(data.ClickedNode);

                            if (baseNode != null)
                            {

                                bool canAddSeparator = true;

                                //Draw specifics node type actions
                                switch (baseNode.BehaviourNodeType)
                                {
                                    case BehaviourNodeType.Action:
                                        canAddSeparator = false;
                                        break;
                                    case BehaviourNodeType.Condition:
                                        canAddSeparator = false;
                                        break;
                                    case BehaviourNodeType.Sequence:
                                        {
                                            SequenceBehaviourNode sequenceBehaviourNode = baseNode as SequenceBehaviourNode;
                                            menu.AddItem(new GUIContent("Add sequence output"), false, () =>
                                            {
                                                sequenceBehaviourNode.AddSequenceOutput();
                                            });

                                            menu.AddItem(new GUIContent("Remove Sequence output"), false, () =>
                                            {
                                                sequenceBehaviourNode.RemoveSequenceOutput();
                                            });
                                        }
                                        break;
                                }

                                if (canAddSeparator)
                                    menu.AddSeparator("");
                            }

                            data.DeleteNode = false;

                        }

                        menu.AddItem(new GUIContent("Add Condition Node"), false, () =>
                       {
                           var node = data.Behaviour.CreateNode<ConditionBehaviourNode>(typeof(ConditionBehaviourNode));
                           node.InEditorWindow.NodePosition = e.mousePosition;
                       });
                        menu.AddItem(new GUIContent("Add Action Node"), false, () =>
                    {
                        var node = data.Behaviour.CreateNode<ActionBehaviourNode>(typeof(ActionBehaviourNode));
                        node.InEditorWindow.NodePosition = e.mousePosition;
                    });
                        menu.AddItem(new GUIContent("Add Sequence Node"), false, () =>
                      {
                          var node = data.Behaviour.CreateNode<SequenceBehaviourNode>(typeof(SequenceBehaviourNode));
                          node.InEditorWindow.NodePosition = e.mousePosition;
                      });

                        menu.ShowAsContext();
                        data.CurrentEvent = EditorData.InputEventType.None;
                    }
                    break;
                case EditorData.InputEventType.CreatingConnection:
                    {
                        if (e == null)
                        {
                            break;
                        }

                        if (e.keyCode == KeyCode.Escape) 
                        {
                            ClearFromCreatintNodeOutput();
                            data.FromCreatingNode = string.Empty;
                            break;
                        }

                        int fromOutPutId = data.FromCreatingNodeOutputID;
                        BehaviourNode fromNode = data.ClickedNode != data.Behaviour.RootNode.NodeID ? data.Behaviour.GetBaseNode(data.ClickedNode) : data.Behaviour.RootNode;
                        Vector3 outputPosition = GetOutputPosition(fromNode, data.FromCreatingNodeOutputID);
                        Handles.DrawBezier(outputPosition, e.mousePosition, outputPosition + Vector3.right * 50, (Vector3)e.mousePosition + Vector3.left * 50, Color.black, null, 2);
                    }
                    break;
                case EditorData.InputEventType.CreateConnection:
                    {
                        BehaviourNode fromNode = data.FromCreatingNode != data.Behaviour.RootNode.NodeID ? data.Behaviour.GetBaseNode(data.FromCreatingNode) : data.Behaviour.RootNode;

                        switch (fromNode.BehaviourNodeType)
                        {
                            case BehaviourNodeType.Root:
                                {
                                    RootBehaviourNode root = fromNode as RootBehaviourNode;
                                    data.Behaviour.RootNode.SetOutputChildId(data.ClickedNode);
                                }
                                break;
                            case BehaviourNodeType.Condition:
                                {
                                    ConditionBehaviourNode conditionNode = fromNode as ConditionBehaviourNode;
                                    if (data.FromCreatingNodeOutputID == 0)
                                    {
                                        conditionNode.SetFalseChildId(data.ClickedNode);
                                    }
                                    else 
                                    {
                                        conditionNode.SetTrueChildId(data.ClickedNode);
                                    }
                                }
                                break;
                            case BehaviourNodeType.Action:
                                {
                                    ActionBehaviourNode actionNode = fromNode as ActionBehaviourNode;
                                    actionNode.SetOutputChild(data.ClickedNode);
                                }
                                break;
                            case BehaviourNodeType.Sequence:
                                {
                                    SequenceBehaviourNode sequenceNode = fromNode as SequenceBehaviourNode;
                                    sequenceNode.SetChild(data.FromCreatingNodeOutputID, data.ClickedNode);
                                }
                                break;
                        }

                        BehaviourNode toNode = data.Behaviour.GetBaseNode(data.ClickedNode);
                        toNode.ParentId = data.FromCreatingNode;
                        data.CurrentEvent = EditorData.InputEventType.None;
                    }
                    break;
                case EditorData.InputEventType.Drag:
                    {
                        data.CurrentEvent = EditorData.InputEventType.None;
                    }
                    break;
            }

        }

        bool IsLeftMouseButtenEvent(Event e)
        {
            return e.button == 0;
        }

        bool IsRightMouseButtonEvent(Event e)
        {
            return e.button == 1;
        }

        bool IsMiddleMouseButtonEvent(Event e) 
        {
            return e.button == 2;
        }

        #endregion

        #region - Draw - 
        public void OnGUI()
        {
            if (data == null)
            {
                bool action = EditorUtility.DisplayDialog("Editor script recompiled.", "The editor script was recompiled, EditorData data is null. Reload data...", "Reload");
                if (action)
                {
                    data = new EditorData();
                }
                else
                {
                    Close();
                    return;
                }
            }

            try
            {
                data.Behaviour = (PTreeBehaviour)Selection.activeObject;
            }
            catch
            {
                data.Behaviour = null;
            }

            if (data.Behaviour == null)
            {
                GUILayout.Label("Select a behaviour to edit", AddMargin(new RectOffset(0, 0, Mathf.RoundToInt(maxSize.y * .5f), 0), CenterText(h2())));
                return;
            }

            Event e = Event.current;

            if (e != null)
            {
                DetectEvents(e);
            }

            ManageEvents(e);

            Draw();
            Repaint();
        }

        void Draw()
        {
            DrawBehaviourName();
            DrawNodeContainer();
            DrawConnectors();
            DrawNodeInspector();
        }

        void DrawBehaviourName(bool debug = false)
        {
            GUILayout.BeginHorizontal();
            {
                if (!data.ChangingTitle)
                {
                    GUILayout.Label(data.Behaviour.Name, AddMargin(new RectOffset(50, 0, 50, 0), h1()));
                }
                else
                {
                    GUIStyle textFiedStyle = new GUIStyle(GUI.skin.textField);
                    textFiedStyle.fontSize = h1().fontSize;

                    data.Behaviour.Name = GUILayout.TextField(data.Behaviour.Name, AddMargin(new RectOffset(50, 0, 50, 0), AddPadding(new RectOffset(0, 0, 10, 0), textFiedStyle)), GUILayout.Width(maxSize.x * .45f), GUILayout.Height(50));
                }

                if (GUILayout.Button(data.ChangingTitle ? "✓" : "C", AddMargin(new RectOffset(25, 0, 50, 0), new GUIStyle(GUI.skin.button)), GUILayout.Width(25), GUILayout.Height(25)))
                {
                    data.ChangingTitle = !data.ChangingTitle;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            if (debug)
            {
                GUILayout.Label("===========================================");

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Debug Reset", GUILayout.Width(100)))
                    {
                        data = new EditorData();
                    }
                    if (GUILayout.Button("Clear Nodes", GUILayout.Width(100)))
                    {
                        data.Behaviour.AllNodeIds = new string[0];
                        data.Behaviour.AllConditionNodes = new ConditionBehaviourNode[0];
                        data.Behaviour.AllActionNodes = new ActionBehaviourNode[0];
                        data.Behaviour.AllSequenceNodes = new SequenceBehaviourNode[0];
                    }
                    if (GUILayout.Button("Reset Node Position", GUILayout.Width(100)))
                    {
                        data.ScreenPosition = Vector2.zero;
                        data.Behaviour.RootNode.InEditorWindow.WindowRect.position = maxSize / 2;
                        foreach (var node in data.Behaviour.AllNodes)
                        {
                            node.InEditorWindow.WindowRect.position = maxSize / 2;
                        }
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Label("Clicked Node " + (string.IsNullOrEmpty(data.ClickedNode) ? " " : (data.ClickedNode != data.Behaviour.RootNode.NodeID) ? data.Behaviour.GetBaseNode(data.ClickedNode).GetType().Name : data.Behaviour.RootNode.GetType().Name));
                GUILayout.Label("From Creating Node Output Id " + data.FromCreatingNodeOutputID);
                GUILayout.Label("Screen Position " + data.ScreenPosition);
            }
        }

        void DrawNodeContainer()
        {
            BeginWindows();
            {
                RootBehaviourNode rootNode = data.Behaviour.RootNode;
                NodeGUI<RootBehaviourNode> rootNodeGui = new NodeGUI<RootBehaviourNode>(data, rootNode, "Root");
                rootNode.InEditorWindow.WindowRect = rootNodeGui.Draw(0);

                if (Event.current != null && IsLeftMouseButtenEvent(Event.current) && string.IsNullOrEmpty(data.ClickedNode)) 
                {
                    rootNode.InEditorWindow.WindowRect.position += Event.current.delta;
                }
                
                int id = 1;
                foreach (var nodeId in data.Behaviour.AllNodeIds)
                {
                    var baseNode = data.Behaviour.GetBaseNode(nodeId);
                    switch (baseNode.BehaviourNodeType)
                    {
                        case BehaviourNodeType.Root:
                            {
                            }
                            break;
                        case BehaviourNodeType.Condition:
                            {
                                ConditionBehaviourNode conditionNode = baseNode as ConditionBehaviourNode;
                                NodeGUI<ConditionBehaviourNode> nodeGui = new NodeGUI<ConditionBehaviourNode>(data, conditionNode, "Condition");
                                conditionNode.InEditorWindow.WindowRect = nodeGui.Draw(id);
                            }
                            break;
                        case BehaviourNodeType.Action:
                            {
                                ActionBehaviourNode actionNode = baseNode as ActionBehaviourNode;
                                NodeGUI<ActionBehaviourNode> nodeGui = new NodeGUI<ActionBehaviourNode>(data, actionNode, "Action");
                                actionNode.InEditorWindow.WindowRect = nodeGui.Draw(id);
                            }
                            break;
                        case BehaviourNodeType.Sequence:
                            {
                                SequenceBehaviourNode sequenceNode = baseNode as SequenceBehaviourNode;
                                NodeGUI<SequenceBehaviourNode> nodeGui = new NodeGUI<SequenceBehaviourNode>(data, sequenceNode, "Sequence");
                                sequenceNode.InEditorWindow.WindowRect = nodeGui.Draw(id);
                            }
                            break;
                    }

                    if (Event.current != null && IsLeftMouseButtenEvent(Event.current) && string.IsNullOrEmpty(data.ClickedNode)) 
                    {
                        baseNode.InEditorWindow.WindowRect.position += Event.current.delta;
                    }

                    id++;
                }
            }
            EndWindows();
            
            EditorUtility.SetDirty(data.Behaviour);
        }

        void DrawConnectors()
        {
            if (!string.IsNullOrEmpty(data.Behaviour.RootNode.OutputId))
            {
                if (!Array.Exists(data.Behaviour.AllNodeIds, x => x == data.Behaviour.RootNode.OutputId))
                {
                    data.Behaviour.RootNode.SetOutputChildId(string.Empty);
                }
                else
                {
                    BehaviourNode toNode = data.Behaviour.GetBaseNode(data.Behaviour.RootNode.OutputId);
                    Vector2 fromPosition = GetOutputPosition(data.Behaviour.RootNode, 0);
                    Handles.DrawBezier(fromPosition, toNode.InEditorWindow.InputPosition, (Vector3)fromPosition + Vector3.right * 50, (Vector3)toNode.InEditorWindow.InputPosition + Vector3.left * 50, Color.black, null, 2);
                }
            }

            foreach (var nodeId in data.Behaviour.AllNodeIds)
            {
                var baseNode = data.Behaviour.GetBaseNode(nodeId);
                switch (baseNode.BehaviourNodeType)
                {
                    case BehaviourNodeType.Condition:
                        {
                            ConditionBehaviourNode conditionNode = baseNode as ConditionBehaviourNode;

                            if (!string.IsNullOrEmpty(conditionNode.FalseNode))
                            {
                                if (!Array.Exists(data.Behaviour.AllNodeIds, x => x == conditionNode.FalseNode))
                                {
                                    conditionNode.SetFalseChildId(string.Empty);
                                    break;
                                }

                                BehaviourNode toFalseNode = data.Behaviour.GetBaseNode(conditionNode.FalseNode);
                                Vector2 fromPosition = GetOutputPosition(conditionNode, 0);
                                Handles.DrawBezier(fromPosition, toFalseNode.InEditorWindow.InputPosition, (Vector3)fromPosition + Vector3.right * 50, (Vector3)toFalseNode.InEditorWindow.InputPosition + Vector3.left * 50, Color.black, null, 2);
                            }
                            if (!string.IsNullOrEmpty(conditionNode.TrueNode))
                            {
                                if (!Array.Exists(data.Behaviour.AllNodeIds, x => x == conditionNode.TrueNode))
                                {
                                    conditionNode.SetTrueChildId(string.Empty);
                                    break;
                                }

                                BehaviourNode toTrueNode = data.Behaviour.GetBaseNode(conditionNode.TrueNode);
                                Vector2 fromPosition = GetOutputPosition(conditionNode, 1);
                                Handles.DrawBezier(fromPosition, toTrueNode.InEditorWindow.InputPosition, (Vector3)fromPosition + Vector3.right * 50, (Vector3)toTrueNode.InEditorWindow.InputPosition + Vector3.left * 50, Color.black, null, 2);
                            }
                        }
                        break;
                    case BehaviourNodeType.Action:
                        {
                            ActionBehaviourNode actionNode = baseNode as ActionBehaviourNode;
                            
                            if (!string.IsNullOrEmpty(actionNode.OutputId))
                            {
                                if (!Array.Exists(data.Behaviour.AllNodeIds, x => x == actionNode.OutputId))
                                {
                                    actionNode.SetOutputChild(string.Empty);
                                    break;
                                }

                                BehaviourNode toNode = data.Behaviour.GetBaseNode(actionNode.OutputId);
                                Vector2 fromPosition = GetOutputPosition(actionNode, 0);
                                Handles.DrawBezier(fromPosition, toNode.InEditorWindow.InputPosition, (Vector3)fromPosition + Vector3.right * 50, (Vector3)toNode.InEditorWindow.InputPosition + Vector3.left * 50, Color.black, null, 2);
                            }
                        }
                        break;
                    case BehaviourNodeType.Sequence:
                        {
                            SequenceBehaviourNode sequenceNode = baseNode as SequenceBehaviourNode;

                            for (int i = 0; i < sequenceNode.Outputs.Length; i++)
                            {
                                string outputId = sequenceNode.Outputs[i];

                                if (string.IsNullOrEmpty(outputId)) 
                                {
                                    continue;
                                }

                                if (!Array.Exists(data.Behaviour.AllNodeIds, x => x == outputId))
                                {
                                    sequenceNode.SetChild(i, string.Empty);
                                    continue;
                                }

                                BehaviourNode toNode = data.Behaviour.GetBaseNode(outputId);
                                Vector2 fromPosition = GetOutputPosition(sequenceNode, i);
                                Handles.DrawBezier(fromPosition, toNode.InEditorWindow.InputPosition, (Vector3)fromPosition + Vector3.right * 50, (Vector3)toNode.InEditorWindow.InputPosition + Vector3.left * 50, Color.black, null, 2);
                            } 
                        }
                        break;
                }
            }
        }

        void DrawNodeInspector()
        {
            BehaviourNode node = data.Behaviour.GetBaseNode(data.ClickedNode);
            
            if (node == null || node != null && node.BehaviourNodeType != BehaviourNodeType.Action && node.BehaviourNodeType != BehaviourNodeType.Condition) 
            {
                return;
            }

            GUILayout.BeginArea(new Rect(new Vector2(0, 100), new Vector2(250, maxSize.y - 100)), AddPadding(new RectOffset(10, 0, 0, 0), new GUIStyle(GUI.skin.window)));
            {
                GUILayout.Label(node.BehaviourNodeType.ToString(), AddPadding(new RectOffset(10, 0, 10, 0), new GUIStyle(GUI.skin.label)));

                BehaviourNodeParameter parameter = null;
                SerializedObject serializedParameter = null;

                switch (node.BehaviourNodeType) 
                {
                    case BehaviourNodeType.Condition:
                        parameter = (node as ConditionBehaviourNode).ConditionNodeParameter;
                        parameter = parameter as ConditionNodeParameter;
                        break;
                    case BehaviourNodeType.Action:
                        parameter = (node as ActionBehaviourNode).ActionNodeParameter;
                        parameter = parameter as ActionNodeParameter;
                        break;
                }

                if (parameter == null) 
                {
                    GUILayout.EndArea();
                    return;
                }

                var type = parameter.GetType();
                foreach (var field in type.GetFields(BindingFlags.NonPublic)) 
                {
                    Debug.Log(field.Name);
                }
            }
            GUILayout.EndArea();
        }

        #endregion

        #region - GUIStyles - 

        public GUIStyle h1() 
        {
            GUIStyle h1 = new GUIStyle();
            h1.fontSize = 24;
            h1.normal.textColor = Color.white;
            return h1;
        }

        public GUIStyle h2()
        {
            GUIStyle h2 = new GUIStyle();
            h2.fontSize = 18;
            h2.normal.textColor = Color.white;
            return h2;
        }

        public GUIStyle CenterText(GUIStyle target) 
        {
            target.alignment = TextAnchor.MiddleCenter;
            return target;
        }

        public GUIStyle AddMargin(RectOffset margin, GUIStyle target) 
        {
            target.margin = margin;
            return target;
        }

        public GUIStyle AddPadding(RectOffset padding, GUIStyle target) 
        {
            target.padding = padding;
            return target;
        }
        #endregion

        #region - Utils - 

        bool IsInNode(Vector2 position, out string nodeId) 
        {
            foreach (var node in data.Behaviour.AllNodes) 
            {
                if (node.InEditorWindow.WindowRect.Contains(position)) 
                {
                    data.FromCreatingNode = data.ClickedNode;
                    nodeId = node.NodeID;
                    return true;
                }
            }

            if (data.Behaviour.RootNode.InEditorWindow.WindowRect.Contains(position)) 
            {
                nodeId = data.Behaviour.RootNode.NodeID;
                return true;
            }


            nodeId = string.Empty;

            return false;
        }

        Vector2 GetOutputPosition(BehaviourNode node, int id = 0)
        {
            Vector2 outputPostion = node.InEditorWindow.WindowRect.position;
            outputPostion.x += node.InEditorWindow.WindowRect.size.x - 10;
;
            float offset = (id == 0) ? 1 : 20;
            outputPostion.y += 35 + (offset * id);

            return outputPostion;
        }

        void ClearFromCreatintNodeOutput() 
        {
            BehaviourNode node = (data.ClickedNode == data.Behaviour.RootNode.NodeID) ? data.Behaviour.RootNode : null;

            if (node == null) 
            {
                node = data.Behaviour.GetBaseNode(data.FromCreatingNode);
            }

            switch (node.BehaviourNodeType) 
            {
                case BehaviourNodeType.Root:
                    {
                        (node as RootBehaviourNode).SetOutputChildId(string.Empty);
                    }
                    break;
                case BehaviourNodeType.Action:
                    {
                        (node as ActionBehaviourNode).SetOutputChild(string.Empty);
                    }
                    break;
                case BehaviourNodeType.Condition:
                    {
                        if (data.FromCreatingNodeOutputID == 0)
                        {
                            (node as ConditionBehaviourNode).SetFalseChildId(string.Empty);
                        }
                        else 
                        {
                            (node as ConditionBehaviourNode).SetTrueChildId(string.Empty);
                        }
                    }
                    break;
                case BehaviourNodeType.Sequence:
                    {
                        (node as SequenceBehaviourNode).SetChild(data.FromCreatingNodeOutputID, string.Empty);
                    }
                    break;
            }
        }

        #endregion
    }
}