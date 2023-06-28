using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace VaporKeys
{
    [CreateAssetMenu(menuName = "Vapor/Config/Enum Rebuilder", order = 11)]
    public class EnumRebuilder : ScriptableObject
    {
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties"), FolderPath]
#endif
        public string DefinitionPath;
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties"), FolderPath(ParentFolder = "Assets")]
#endif
        public string FolderPath;
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties")]
#endif
        public string NamespaceName;
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties")]
#endif
        public string DefinitionName;
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties")]
#endif
        public bool CustomOrder;
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties")]
#endif
        public bool CreateNone;
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties"), ShowIf("@CustomOrder")]
#endif
        public int StartingValue;
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties"), ShowIf("@CustomOrder")]
#endif
        public EnumGeneratorConfig.OrderDirection OrderDirection;
#if ODIN_INSPECTOR
        [BoxGroup("Enum Properties")]
#endif
        public string TypeRebuild;

        [Button]
        private void Rebuild()
        {
            var t = Type.GetType($"{TypeRebuild}");
            var fi = t.GetField("DropdownValues");
            var fiO = fi.GetValue(null);
            List<string> enumContent = new ();
            if(fiO is ValueDropdownList<KeyDropdownValue> vdl)
            {

                foreach (var item in vdl)
                {
                    enumContent.Add(item.Text);
                    Debug.Log($"{item.Text} {item.Value.Key}");
                }
            }

            ScriptableDefinition def = new()
            {
                folderPath = FolderPath,
                namespaceName = NamespaceName,
                definitionName = DefinitionName,
                customOrder = CustomOrder,
                startingValue = StartingValue,
                orderDirection = (int)OrderDirection,
                createNone = CreateNone,
                enumContent = new List<string>(enumContent)
            };

            AssetDatabase.CreateAsset(def, $"{DefinitionPath}/{DefinitionName}.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
