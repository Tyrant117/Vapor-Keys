#if UNITY_EDITOR
using UnityEditor;
#endif
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;

namespace VaporKeys
{

    [CreateAssetMenu(menuName = "Vapor/Config/Enum Generator Config", order = 10)]
    public class EnumGeneratorConfig : ScriptableObject
    {
        public enum OrderDirection
        {
            CountUp,
            CountDown
        }

#if ODIN_INSPECTOR
        [BoxGroup("Config Properties"), FolderPath]
#endif
        public string definitionPath;
#if ODIN_INSPECTOR
        [BoxGroup("Config Properties"), InlineButton("Load")]
#endif
        public ScriptableDefinition loadDefinition;
        private void Load()
        {
#if UNITY_EDITOR
            folderPath = loadDefinition.folderPath;
            namespaceName = loadDefinition.namespaceName;
            definitionName = loadDefinition.definitionName;
            customOrder = loadDefinition.customOrder;
            startingValue = loadDefinition.startingValue;
            orderDirection = (OrderDirection)loadDefinition.orderDirection;
            useInternalID = loadDefinition.useInternalID;
            enumContent = new List<string>(loadDefinition.enumContent);
            loadDefinition = null;
            AssetDatabase.Refresh();
#endif
        }

#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties"), FolderPath(ParentFolder = "Assets")]
#endif
        public string folderPath;
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties")]
#endif
        public string namespaceName;
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties")]
#endif
        public string definitionName;
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties")]
#endif
        public bool customOrder;
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties")]
#endif
        public bool useInternalID;
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties"), ShowIf("@customOrder")]
#endif
        public int startingValue;
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties"), ShowIf("@customOrder")]
#endif
        public OrderDirection orderDirection;
#if ODIN_INSPECTOR
        [Searchable, BoxGroup("Enum Properties")]
#endif
        public List<string> enumContent;

#if ODIN_INSPECTOR
        [PropertySpace, Button("Create")]
#endif
        private void Create()
        {
#if UNITY_EDITOR

            var kvp = new List<KeyGenerator.KeyValuePair>();
            int currentValue = startingValue;
            foreach (var name in enumContent)
            {
                if (customOrder)
                {
                    kvp.Add(new KeyGenerator.KeyValuePair(name, currentValue, Regex.Replace(name, " ", "_"), useInternalID));
                    switch (orderDirection)
                    {
                        case OrderDirection.CountUp:
                            currentValue++;
                            break;
                        case OrderDirection.CountDown:
                            currentValue--;
                            break;
                    }
                }
                else
                {
                    kvp.Add(new KeyGenerator.KeyValuePair(name, name.GetHashCode(), Regex.Replace(name, " ", "_"), useInternalID));
                }
            }

            KeyGenerator.FormatKeyFiles(folderPath, namespaceName, definitionName, kvp);

            ScriptableDefinition def = new();
            def.folderPath = folderPath;
            def.namespaceName = namespaceName;
            def.definitionName = definitionName;
            def.customOrder = customOrder;
            def.startingValue = startingValue;
            def.orderDirection = (int)orderDirection;
            def.useInternalID = useInternalID;
            def.enumContent = new List<string>(enumContent);

            AssetDatabase.CreateAsset(def, $"{definitionPath}/{definitionName}.asset");

            definitionName = "";
            enumContent.Clear();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
#endif
        }
    }
}
