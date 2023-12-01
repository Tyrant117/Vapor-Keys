using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VaporKeys
{
    [System.Serializable]
    public class JSONConfigDefinition
    {
        public string FolderPath;
        public string NamespaceName;
        public string DefinitionName;
        public bool CustomOrder;
        public int StartingValue;
        public int OrderDirection;
        public bool CreateNone;
        public List<string> EnumContent;

        public static void ToJson(JSONConfigDefinition def, string path)
        {
            var jsonString = JsonUtility.ToJson(def, true);
            System.IO.File.WriteAllText(path, jsonString);
        }

        public static JSONConfigDefinition FromJson(string path, string filename)
        {
            var jsonString = System.IO.File.ReadAllText($"{path}/{filename}");
            return JsonUtility.FromJson<JSONConfigDefinition>(jsonString);
        }
    }
}
