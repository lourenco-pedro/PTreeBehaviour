using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PTreeBehaviour
{
    public class BehaviourNodeParameter : ScriptableObject
    {
        public string Name => _name;

        [SerializeField]
        protected string _name;
    }
}