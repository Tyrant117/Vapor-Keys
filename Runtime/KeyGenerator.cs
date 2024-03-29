#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace VaporKeys
{
    public class KeyGenerator
    {
        public const string AbsoluteConfigPath = "Assets/Vapor Keys/Config";
        public const string RelativeConfigPath = "Vapor Keys/Config";
        public const string AbsoluteKeyPath = "Assets/Vapor Keys/Keys";
        public const string RelativeKeyPath = "Vapor Keys/Keys";
        public const string NamespaceName = "VaporKeyDefinitions";

        #region - Keys -
        public struct KeyValuePair
        {
            public string displayName;
            public string variableName;
            public string guid;
            public int key;

            public KeyValuePair(string name, int key, string guid)
            {
                this.displayName = name;
                this.variableName = Regex.Replace(name, " ", "");
                this.guid = guid;
                this.key = key;
            }

            public KeyValuePair(IKey key)
            {
                this.displayName = key.DisplayName;
                this.variableName = Regex.Replace(key.DisplayName, " ", "");
                key.ForceRefreshKey();
                guid = string.Empty;
#if UNITY_EDITOR
                if(key is Object so)
                {
                    guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(so));
                    EditorUtility.SetDirty(so);
                }
#endif
                this.key = key.Key;
            }

            public string GetFormat(int placeholderIndex)
            {
                string vName = variableName.Length > 0 ? variableName : "Placeholder_" + placeholderIndex;
                return $"public const int {vName} = {key};";
            }

            private static string InsertSpaceBeforeUpperCase(string str)
            {
                var sb = new StringBuilder();

                char previousChar = char.MinValue; // Unicode '\0'

                foreach (char c in str)
                {
                    if (char.IsUpper(c))
                    {
                        // If not the first character and previous character is not a space, insert a space before uppercase

                        if (sb.Length != 0 && previousChar != ' ')
                        {
                            sb.Append(' ');
                        }
                    }

                    sb.Append(c);

                    previousChar = c;
                }

                return sb.ToString();
            }
        }

        public static KeyValuePair StringToKeyValuePair(string key)
        {
            return new KeyValuePair(key, key.GetKeyHashCode(), string.Empty);
        }

#if UNITY_EDITOR
        public static void GenerateKeys(Type typeFilter, string scriptName, bool includeNone)
        {
            var guids = AssetDatabase.FindAssets($"t:{typeFilter.Name}");
            HashSet<int> takenKeys = new();
            List<KeyValuePair> formattedKeys = new();

            if (includeNone)
            {
                takenKeys.Add(0);
                formattedKeys.Add(new KeyValuePair("None", 0, string.Empty));
            }

            foreach (var item in GetAllAssets<Object>(guids))
            {
                if (item == null) { continue; }
                if (item is not IKey key) { continue; }
                if (key.IsDeprecated) { continue; }

                key.ForceRefreshKey();
                if (takenKeys.Contains(key.Key))
                {
                    Debug.LogError($"Key Collision: {item.name}. Objects cannot share a name.");
                }
                else
                {
                    EditorUtility.SetDirty(item);
                    takenKeys.Add(key.Key);
                    var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(item));
                    formattedKeys.Add(new KeyValuePair(item.name, key.Key, guid));
                }
            }

            FormatKeyFiles(RelativeKeyPath, NamespaceName, scriptName, formattedKeys);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void GenerateKeys(string searchFilter, string scriptName, bool includeNone)
        {
            HashSet<int> takenKeys = new();
            List<KeyValuePair> formattedKeys = new();

            if(includeNone)
            {
               takenKeys.Add(0);
               formattedKeys.Add(new KeyValuePair("None", 0, string.Empty));     
            }

            List<string> guids = new();
            guids.AddRange(AssetDatabase.FindAssets(searchFilter));
            for (int i = 0; i < guids.Count; i++)
            {
                var refVal = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (refVal == null) { continue; }
                if (refVal is IKey rfk && rfk.IsDeprecated) { continue; }

                var key = refVal.name.GetKeyHashCode();
                if (takenKeys.Contains(key))
                {
                    Debug.LogError($"Key Collision: {refVal.name}. Objects cannot share a name.");
                }
                else
                {
                    takenKeys.Add(key);
                    if (refVal is Object so)
                    {
                        var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(so));
                        formattedKeys.Add(new KeyValuePair(refVal.name, key, guid));
                    }
                    else
                    {
                        formattedKeys.Add(new KeyValuePair(refVal.name, key, string.Empty));
                    }
                }                
            }

            FormatKeyFiles(RelativeKeyPath, NamespaceName, scriptName, formattedKeys);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void GenerateKeys<T>(string scriptName, bool includeNone) where T : ScriptableObject, IKey
        {
            string typeFilter = typeof(T).Name;
            Debug.Log($"Generating Keys of Type: {typeFilter}");
            GenerateKeys<T>(AssetDatabase.FindAssets($"t:{typeFilter}"), scriptName, includeNone);
        }

        public static void GenerateKeys<T>(string[] guids, string scriptName, bool includeNone) where T : ScriptableObject, IKey
        {
            HashSet<int> takenKeys = new();
            List<KeyValuePair> formattedKeys = new();

            if(includeNone)
            {
               takenKeys.Add(0);
               formattedKeys.Add(new KeyValuePair("None", 0, string.Empty));     
            }

            foreach (var item in GetAllAssets<T>(guids))
            {
                if (item == null) { continue; }
                if (item.IsDeprecated) { continue; }

                item.ForceRefreshKey();
                if (takenKeys.Contains(item.Key))
                {
                    Debug.LogError($"Key Collision: {item.name}. Objects cannot share a name.");
                }
                else
                {
                    EditorUtility.SetDirty(item);
                    takenKeys.Add(item.Key);
                    var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(item));
                    formattedKeys.Add(new KeyValuePair(item.name, item.Key, guid));
                }
            }

            FormatKeyFiles(RelativeKeyPath, NamespaceName, scriptName, formattedKeys);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void AddKey(Type type, string relativePath, List<(string, int)> values, IKey keyToAdd)
        {
            List<KeyValuePair> kvps = new();
            KeyValuePair newKvp = new(keyToAdd);
            foreach (var value in values)
            {
                if (value.Item2 == newKvp.key)
                {
                    Debug.LogError($"Key Collision: {value.Item1}. Objects cannot share a name.");
                    return;
                }
                if (keyToAdd is Object so)
                {
                    var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(so));
                    kvps.Add(new(value.Item1, value.Item2, guid));
                }
                else
                {
                    kvps.Add(new(value.Item1, value.Item2, string.Empty));
                }
            }
            kvps.Add(newKvp);

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void AddKey(Type type, string relativePath, List<(string, KeyDropdownValue)> values, string keyToAdd)
        {
            List<KeyValuePair> kvps = new();
            KeyValuePair newKvp = StringToKeyValuePair(keyToAdd);
            foreach (var value in values)
            {
                if (value.Item2.Key == newKvp.key)
                {
                    Debug.LogError($"Key Collision: {value.Item1}. Objects cannot share a name.");
                    return;
                }
                kvps.Add(new(value.Item1, value.Item2.Key, string.Empty));
            }
            kvps.Add(newKvp);

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void AddKeys(Type type, string relativePath, List<(string, KeyDropdownValue)> values, IKey[] keysToAdd)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> newKvps = new();
            foreach (var keyToAdd in keysToAdd)
            {
                newKvps.Add(new(keyToAdd));
            }
            foreach (var value in values)
            {
                bool match(KeyValuePair x) => x.key == value.Item2.Key;
                if (newKvps.Exists(match))
                {
                    Debug.LogError($"Key Collision: {value.Item1}. Objects cannot share a name.");
                    return;
                }
                kvps.Add(new(value.Item1, value.Item2.Key, value.Item2.Guid));
            }
            foreach (var newKvp in newKvps)
            {
                kvps.Add(newKvp);
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void AddKeys(Type type, string relativePath, List<(string, KeyDropdownValue)> values, string[] keysToAdd)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> newKvps = new();
            foreach (var keyToAdd in keysToAdd)
            {
                newKvps.Add(StringToKeyValuePair(keyToAdd));
            }
            foreach (var value in values)
            {
                bool match(KeyValuePair x) => x.key == value.Item2.Key;
                if (newKvps.Exists(match))
                {
                    Debug.LogError($"Key Collision: {value.Item1}. Objects cannot share a name.");
                    return;
                }
                kvps.Add(new(value.Item1, value.Item2.Key, string.Empty));
            }
            foreach (var newKvp in newKvps)
            {
                kvps.Add(newKvp);
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveKey(Type type, string relativePath, List<(string, KeyDropdownValue)> values, IKey keyToRemove)
        {
            List<KeyValuePair> kvps = new();
            KeyValuePair removeKvp = new(keyToRemove);
            foreach (var value in values)
            {
                if (value.Item2.Key != removeKvp.key)
                {
                    kvps.Add(new(value.Item1, value.Item2.Key, value.Item2.Guid));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveKey(Type type, string relativePath, List<(string, KeyDropdownValue)> values, string keyToRemove)
        {
            List<KeyValuePair> kvps = new();
            KeyValuePair removeKvp = StringToKeyValuePair(keyToRemove);
            foreach (var value in values)
            {
                if (value.Item2.Key != removeKvp.key)
                {
                    kvps.Add(new(value.Item1, value.Item2.Key, value.Item2.Guid));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveKeys(Type type, string relativePath, List<(string, KeyDropdownValue)> values, IKey[] keysToRemove)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> removeKvps = new();
            foreach (var keyToAdd in keysToRemove)
            {
                removeKvps.Add(new(keyToAdd));
            }
            foreach (var value in values)
            {
                if (!removeKvps.Exists(x => x.key == value.Item2.Key))
                {
                    kvps.Add(new(value.Item1, value.Item2.Key, value.Item2.Guid));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveKeys(Type type, string relativePath, List<(string, KeyDropdownValue)> values, string[] keysToRemove)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> removeKvps = new();
            foreach (var keyToAdd in keysToRemove)
            {
                removeKvps.Add(StringToKeyValuePair(keyToAdd));
            }
            foreach (var value in values)
            {
                if (!removeKvps.Exists(x => x.key == value.Item2.Key))
                {
                    kvps.Add(new(value.Item1, value.Item2.Key, value.Item2.Guid));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveDeprecated(Type type, string relativePath, List<(string, KeyDropdownValue)> values, IKey[] keysToRemove)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> removeKvps = new();
            foreach (var keyToRemove in keysToRemove)
            {
                if (keyToRemove.IsDeprecated)
                {
                    removeKvps.Add(new(keyToRemove));
                }
            }
            foreach (var value in values)
            {
                if (!removeKvps.Exists(x => x.key == value.Item2.Key))
                {
                    kvps.Add(new(value.Item1, value.Item2.Key, value.Item2.Guid));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        private static IEnumerable<T> GetAllAssets<T>(string[] guids) where T : Object
        {
            foreach (var guid in guids)
            {
                yield return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            }
        }
#endif

        public static void FormatKeyFiles(string gameDataFilepath, string namespaceName, string scriptName, List<KeyValuePair> keys)
        {
            string gameDataProjectFilePath = $"/{gameDataFilepath}/{scriptName}.cs";
            string filepath = Application.dataPath + gameDataProjectFilePath;

            StringBuilder sb = new();


            sb.Append("//\t* THIS SCRIPT IS AUTO-GENERATED *\n");
            //sb.Append("#if ODIN_INSPECTOR\n using Sirenix.OdinInspector;\n #endif\n");
            sb.Append("using System;\n");
            sb.Append("using VaporKeys;\n");
            sb.Append("using System.Collections.Generic;\n\n");

            sb.Append($"namespace {namespaceName}\n");
            sb.Append("{\n");
            sb.Append($"\tpublic class {scriptName}\n");
            sb.Append("\t{\n");


            FormatFilePath(sb, $"{gameDataFilepath}");
            //FormatAttributeName(sb, $"@{scriptName}.DropdownValues");
            FormatAttributeName(sb, $"{scriptName}");

            FormatEnum(sb, keys);

            FormatOdinDropDown(sb, keys);

            for (int i = 0; i < keys.Count; i++)
            {
                int pIndex = i;
                sb.Append("\t\t");
                sb.Append(keys[i].GetFormat(pIndex));
                sb.Append("\n");
            }

            FormatList(sb, keys);
            FormatLookup(sb);
            FormatGet(sb);

            sb.Append("\t}\n");
            sb.Append("}");

            System.IO.File.WriteAllText(filepath, sb.ToString());
        }

        private static void FormatFilePath(StringBuilder sb, string relativePath)
        {
            sb.Append($"\t\tpublic const string RELATIVE_PATH = \"{relativePath}\";\n");
        }

        private static void FormatAttributeName(StringBuilder sb, string relativePath)
        {
            sb.Append($"\t\tpublic const string AttributeName = \"{relativePath}\";\n");
        }

        private static void FormatEnum(StringBuilder sb, List<KeyValuePair> keys)
        {
            sb.Append($"\t\tpublic enum Enum : int\n");
            sb.Append("\t\t{\n");
            for (int i = 0; i < keys.Count; i++)
            {
                sb.Append($"\t\t\t{keys[i].variableName} = {keys[i].key},\n");
            }
            sb.Append("\t\t}\n\n");

            if (keys.Count <= 32)
            {
                sb.Append($"\t\t[Flags]\n");
                sb.Append($"\t\tpublic enum AsFlags : int\n");
                sb.Append("\t\t{\n");
                sb.Append($"\t\t\tNone = 0,\n");
                int skipNone = 0;
                for (int i = 0; i < keys.Count; i++)
                {
                    if(keys[i].displayName == "None") { skipNone = 1; continue; }

                    int flagID = i - skipNone;
                    sb.Append($"\t\t\t{keys[i].variableName} = 1 << {flagID},\n");
                }
                sb.Append($"\t\t\tEverything = ~0,\n");
                sb.Append("\t\t}\n\n");

                sb.Append($"\t\tpublic static Dictionary<int, int> FlagToValueMap = new()\n");
                sb.Append("\t\t{\n");
                skipNone = 0;
                for (int i = 0; i < keys.Count; i++)
                {
                    if(keys[i].displayName == "None") { skipNone = 1; continue; }

                    int flag = 1 << (i - skipNone);
                    sb.Append($"\t\t\t{{ {flag}, {keys[i].key} }},\n");
                }
                sb.Append("\t\t};\n\n");
            }
        }

        private static void FormatOdinDropDown(StringBuilder sb, List<KeyValuePair> keys)
        {
            //sb.Append("#if ODIN_INSPECTOR\n");
            //sb.Append($"\t\tpublic static ValueDropdownList<KeyDropdownValue> DropdownValues = new()\n");
            //sb.Append("\t\t{\n");
            //for (int i = 0; i < keys.Count; i++)
            //{
            //    sb.Append($"\t\t\t{{ \"{keys[i].displayName}\", new (\"{keys[i].guid}\", {keys[i].variableName}) }},\n");
            //}
            //sb.Append("\t\t};\n");
            //sb.Append("#endif\n\n");

            //sb.Append("#if !ODIN_INSPECTOR\n");
            sb.Append($"\t\tpublic static List<(string, KeyDropdownValue)> DropdownValues = new()\n");
            sb.Append("\t\t{\n");
            for (int i = 0; i < keys.Count; i++)
            {
                sb.Append($"\t\t\tnew (\"{keys[i].displayName}\", new (\"{keys[i].guid}\", {keys[i].variableName})),\n");
            }
            sb.Append("\t\t};\n");
            //sb.Append("#endif\n\n");
        }

        private static void FormatList(StringBuilder sb, List<KeyValuePair> keys)
        {
            sb.Append("\n");
            sb.Append($"\t\tpublic static List<int> Values = new ()\n");
            sb.Append("\t\t{\n");
            for (int i = 0; i < keys.Count; i++)
            {
                sb.Append($"\t\t\t{{ {keys[i].variableName} }},\n");
            }
            sb.Append("\t\t};\n");
        }

        private static void FormatEnumeration(StringBuilder sb, string scriptName)
        {
            sb.Append($"\t\tpublic static IEnumerable<int> EnumerateValues()\n");
            sb.Append("\t\t{\n");

            sb.Append($"\t\t\tvar refVals = (typeof({scriptName})).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(int));\n");
            sb.Append("\n");
            sb.Append($"\t\t\tforeach (var fi in refVals)\n");
            sb.Append("\t\t\t{\n");
            sb.Append($"\t\t\t\tyield return (int)fi.GetRawConstantValue();\n");
            sb.Append("\t\t\t}\n");

            sb.Append("\t\t}\n");
        }

        private static void FormatLookup(StringBuilder sb)
        {
            //sb.Append("#if ODIN_INSPECTOR\n");

            //sb.Append($"\t\tpublic static string Lookup(int id)\n");
            //sb.Append("\t\t{\n");

            //sb.Append($"\t\t\treturn DropdownValues.Find((x) => x.Value.Key == id).Text;\n");

            //sb.Append("\t\t}\n");

            //sb.Append("#endif\n");

            //sb.Append("#if !ODIN_INSPECTOR\n");

            sb.Append($"\t\tpublic static string Lookup(int id)\n");
            sb.Append("\t\t{\n");

            sb.Append($"\t\t\treturn DropdownValues.Find((x) => x.Item2.Key == id).Item1;\n");

            sb.Append("\t\t}\n");

            //sb.Append("#endif\n");
        }

        private static void FormatGet(StringBuilder sb)
        {
            //sb.Append("#if ODIN_INSPECTOR\n");

            //sb.Append($"\t\tpublic static KeyDropdownValue Get(int id)\n");
            //sb.Append("\t\t{\n");

            //sb.Append($"\t\t\treturn DropdownValues.Find((x) => x.Value.Key == id).Value;\n");

            //sb.Append("\t\t}\n");

            //sb.Append("#endif\n");

            //sb.Append("#if !ODIN_INSPECTOR\n");

            sb.Append($"\t\tpublic static KeyDropdownValue Get(int id)\n");
            sb.Append("\t\t{\n");

            sb.Append($"\t\t\treturn DropdownValues.Find((x) => x.Item2.Key == id).Item2;\n");

            sb.Append("\t\t}\n");

            //sb.Append("#endif\n");
        }
        #endregion
    }
}
