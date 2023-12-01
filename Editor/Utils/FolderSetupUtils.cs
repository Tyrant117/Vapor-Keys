using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace VaporKeys.Editor
{
    public class FolderSetupUtils
    {
        [InitializeOnLoadMethod]
        static void SetupFolders()
        {
            AssetDatabase.StartAssetEditing();
            bool changed = false;

            if (!AssetDatabase.IsValidFolder("Assets/Vapor Keys"))
            {
                AssetDatabase.CreateFolder("Assets", "Vapor Keys");
                changed = true;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Vapor Keys/Config"))
            {
                AssetDatabase.CreateFolder("Assets/Vapor Keys", "Config");
                changed = true;
            }

            if (!AssetDatabase.IsValidFolder("Assets/Vapor Keys/Keys"))
            {
                AssetDatabase.CreateFolder("Assets/Vapor Keys", "Keys");
                changed = true;
            }

            //var configGen = AssetDatabase.LoadAssetAtPath<EnumGeneratorConfig>("Assets/Vapor Keys/Enum Generator Config.asset");
            //if (!configGen)
            //{
            //    configGen = ScriptableObject.CreateInstance<EnumGeneratorConfig>();
            //    configGen.name = "Enum Generator Config";

            //    AssetDatabase.CreateAsset(configGen, "Assets/Vapor Keys/Enum Generator Config.asset");
            //    changed = true;
            //}

            var asmdef = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Vapor Keys/VaporKeyDefinitions.asmdef");
            if (!asmdef)
            {
                Debug.Log(Application.dataPath + "/Vapor Keys/VaporKeyDefinitions.asmdef");
                StreamWriter w = new(Application.dataPath + "/Vapor Keys/VaporKeyDefinitions.asmdef");
                string format = 
                    "{\n" +
                    $"\t{"\"name\""}: {"\"VaporKeyDefinitions\""},\n" +
                    $"\t{"\"rootNamespace\""}: {"\"VaporKeyDefinitions\""},\n" +
                    $"\t{"\"references\""}: {"["}\n" +
                    $"\t\t{"\"CarbonFiberGames.VaporKeys\""}\n" +
                    $"\t{"]"},\n" +
                    $"\t{"\"includePlatforms\""}: {"[]"},\n" +
                    $"\t{"\"excludePlatforms\""}: {"[]"},\n" +
                    $"\t{"\"allowUnsafeCode\""}: {"false"},\n" +
                    $"\t{"\"overrideReferences\""}: {"false"},\n" +
                    $"\t{"\"precompiledReferences\""}: {"[]"},\n" +
                    $"\t{"\"autoReferenced\""}: {"true"},\n" +
                    $"\t{"\"defineConstraints\""}: {"[]"},\n" +
                    $"\t{"\"versionDefines\""}: {"[]"},\n" +
                    $"\t{"\"noEngineReferences\""}: {"false"}\n" +
                    "}";
                w.Write(format);
                w.Close();
                changed = true;
            }

            AssetDatabase.StopAssetEditing();
            if (changed)
            {
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }
    }
}
