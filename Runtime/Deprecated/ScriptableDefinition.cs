using System.Collections.Generic;
using UnityEngine;

namespace VaporKeys
{
    public class ScriptableDefinition : ScriptableObject
    {
        public string folderPath;
        public string namespaceName;
        public string definitionName;
        public bool customOrder;
        public int startingValue;
        public int orderDirection;
        public bool createNone;
        public List<string> enumContent;
    }
}
