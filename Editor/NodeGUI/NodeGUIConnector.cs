using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PTreeBehaviour.Editor
{
    public class NodeGUIConnector 
    {
        public string Name;
        public Action<int> OnClick;

        public NodeGUIConnector(string name, Action<int> onClick) 
        {
            Name = name;
            OnClick = onClick;
        }
    }
}