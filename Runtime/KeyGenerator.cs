#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using System.Linq;
#endif

namespace VaporKeys
{
    public class KeyGenerator
    {
        #region - Keys -
        public struct KeyValuePair
        {
            public string displayName;
            public string variableName;
            public int key;
            public string internalID;
            public readonly bool useInternalID;

            public KeyValuePair(string name, int key, string internalID, bool useInternalID)
            {
                this.displayName = name;
                this.variableName = Regex.Replace(name, " ", "");
                this.key = key;
                this.internalID = internalID;
                this.useInternalID = useInternalID;
            }

            public KeyValuePair(IKey key, bool useInternalID)
            {
                this.displayName = key.DisplayName;
                this.variableName = Regex.Replace(key.DisplayName, " ", "");
                this.key = key.Key;
                this.internalID = key.InternalID;
                this.useInternalID = useInternalID;
            }

            public string GetFormat(int placeholderIndex)
            {
                string vName = variableName.Length > 0 ? variableName : "Placeholder_" + placeholderIndex;
                return useInternalID ? $"public const string {vName} = \"{internalID}\";"
                                     : $"public const int {vName} = {key};";
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

        public static KeyValuePair StringToKeyValuePair(string key, bool useInternalID)
        {
            return new KeyValuePair(key, key.GetHashCode(), key, useInternalID);
        }

#if UNITY_EDITOR
        public static void GenerateKeys(string searchFilter, string gameDataFilepath, string namespaceName, string scriptName, bool useInternalID, bool includeNone)
        {
            HashSet<int> takenKeys = new();
            List<KeyValuePair> formattedKeys = new();

            if(includeNone)
            {
               takenKeys.Add("None".GetHashCode());
               formattedKeys.Add(new KeyValuePair("None", "None".GetHashCode(), "None", useInternalID));     
            }

            List<string> guids = new();
            guids.AddRange(AssetDatabase.FindAssets(searchFilter));
            for (int i = 0; i < guids.Count; i++)
            {
                var refVal = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (refVal == null) { continue; }
                if (refVal is IKey rfk && rfk.IsDeprecated) { continue; }

                var key = refVal.name.GetHashCode();
                if (takenKeys.Contains(key))
                {
                    Debug.LogError($"Key Collision: {refVal.name}. Objects cannot share a name.");
                }
                else
                {
                    takenKeys.Add(key);
                    formattedKeys.Add(new KeyValuePair(refVal.name, key, refVal.name, useInternalID));
                }                
            }

            FormatKeyFiles(gameDataFilepath, namespaceName, scriptName, formattedKeys);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void GenerateKeys<T>(string gameDataFilepath, string namespaceName, string scriptName, bool useInternalID, bool includeNone) where T : ScriptableObject, IKey
        {
            string typeFilter = typeof(T).Name;
            Debug.Log($"Generating Keys of Type: {typeFilter}");
            GenerateKeys<T>(AssetDatabase.FindAssets($"t:{typeFilter}"), gameDataFilepath, namespaceName, scriptName, useInternalID, includeNone);
        }

        public static void GenerateKeys<T>(string[] assetPaths, string gameDataFilepath, string namespaceName, string scriptName, bool useInternalID, bool includeNone) where T : ScriptableObject, IKey
        {
            HashSet<int> takenKeys = new();
            List<KeyValuePair> formattedKeys = new();

            if(includeNone)
            {
               takenKeys.Add("None".GetHashCode());
               formattedKeys.Add(new KeyValuePair("None", "None".GetHashCode(), "None", useInternalID));     
            }

            foreach (var item in GetAllAssets<T>(assetPaths))
            {
                if (item == null) { continue; }
                if (item.IsDeprecated) { continue; }

                if (takenKeys.Contains(item.Key))
                {
                    Debug.LogError($"Key Collision: {item.name}. Objects cannot share a name.");
                }
                else
                {
                    takenKeys.Add(item.Key);
                    formattedKeys.Add(new KeyValuePair(item.name, item.Key, item.InternalID, useInternalID));
                }
            }

            FormatKeyFiles(gameDataFilepath, namespaceName, scriptName, formattedKeys);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

#if ODIN_INSPECTOR
        public static void AddKey(Type type, string relativePath, ValueDropdownList<int> values, IKey keyToAdd)
        {
            List<KeyValuePair> kvps = new();
            KeyValuePair newKvp = new(keyToAdd, false);
            foreach (var value in values)
            {
                if (value.Value == newKvp.key)
                {
                    Debug.LogError($"Key Collision: {value.Text}. Objects cannot share a name.");
                    return;
                }
                kvps.Add(new(value.Text, value.Value, value.Text, false));
            }
            kvps.Add(newKvp);

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void AddKey(Type type, string relativePath, ValueDropdownList<string> values, IKey keyToAdd)
        {
            List<KeyValuePair> kvps = new();
            KeyValuePair newKvp = new(keyToAdd, true);
            foreach (var value in values)
            {
                if (value.Value == newKvp.internalID)
                {
                    Debug.LogError($"Key Collision: {value.Text}. Objects cannot share a name.");
                    return;
                }
                kvps.Add(new(value.Text, value.Value.GetHashCode(), value.Text, false));
            }
            kvps.Add(newKvp);

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void AddKey(Type type, string relativePath, ValueDropdownList<int> values, string keyToAdd)
        {
            List<KeyValuePair> kvps = new();
            KeyValuePair newKvp = StringToKeyValuePair(keyToAdd, false);
            foreach (var value in values)
            {
                if (value.Value == newKvp.key)
                {
                    Debug.LogError($"Key Collision: {value.Text}. Objects cannot share a name.");
                    return;
                }
                kvps.Add(new(value.Text, value.Value, value.Text, false));
            }
            kvps.Add(newKvp);

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void AddKey(Type type, string relativePath, ValueDropdownList<string> values, string keyToAdd)
        {
            List<KeyValuePair> kvps = new();
            KeyValuePair newKvp = StringToKeyValuePair(keyToAdd, true);
            foreach (var value in values)
            {
                if (value.Value == newKvp.internalID)
                {
                    Debug.LogError($"Key Collision: {value.Text}. Objects cannot share a name.");
                    return;
                }
                kvps.Add(new(value.Text, value.Value.GetHashCode(), value.Value, true));
            }
            kvps.Add(newKvp);

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void AddKeys(Type type, string relativePath, ValueDropdownList<int> values, IKey[] keysToAdd)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> newKvps = new();
            foreach (var keyToAdd in keysToAdd)
            {
                newKvps.Add(new(keyToAdd, false));
            }
            foreach (var value in values)
            {
                bool match(KeyValuePair x) => x.key == value.Value;
                if (newKvps.Exists(match))
                {
                    Debug.LogError($"Key Collision: {value.Text}. Objects cannot share a name.");
                    return;
                }
                kvps.Add(new(value.Text, value.Value, value.Text, false));
            }
            foreach (var newKvp in newKvps)
            {
                kvps.Add(newKvp);
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void AddKeys(Type type, string relativePath, ValueDropdownList<string> values, IKey[] keysToAdd)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> newKvps = new();
            foreach (var keyToAdd in keysToAdd)
            {
                newKvps.Add(new(keyToAdd, true));
            }
            foreach (var value in values)
            {
                bool match(KeyValuePair x) => x.internalID == value.Value;
                if (newKvps.Exists(match))
                {
                    Debug.LogError($"Key Collision: {value.Text}. Objects cannot share a name.");
                    return;
                }
                kvps.Add(new(value.Text, value.Value.GetHashCode(), value.Text, false));
            }
            foreach (var newKvp in newKvps)
            {
                kvps.Add(newKvp);
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void AddKeys(Type type, string relativePath, ValueDropdownList<int> values, string[] keysToAdd)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> newKvps = new();
            foreach (var keyToAdd in keysToAdd)
            {
                newKvps.Add(StringToKeyValuePair(keyToAdd, false));
            }
            foreach (var value in values)
            {
                bool match(KeyValuePair x) => x.key == value.Value;
                if (newKvps.Exists(match))
                {
                    Debug.LogError($"Key Collision: {value.Text}. Objects cannot share a name.");
                    return;
                }
                kvps.Add(new(value.Text, value.Value, value.Text, false));
            }
            foreach (var newKvp in newKvps)
            {
                kvps.Add(newKvp);
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void AddKeys(Type type, string relativePath, ValueDropdownList<string> values, string[] keysToAdd)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> newKvps = new();
            foreach (var keyToAdd in keysToAdd)
            {
                newKvps.Add(StringToKeyValuePair(keyToAdd, true));
            }
            foreach (var value in values)
            {
                bool match(KeyValuePair x) => x.internalID == value.Value;
                if (newKvps.Exists(match))
                {
                    Debug.LogError($"Key Collision: {value.Text}. Objects cannot share a name.");
                    return;
                }
                kvps.Add(new(value.Text, value.Value.GetHashCode(), value.Text, false));
            }
            foreach (var newKvp in newKvps)
            {
                kvps.Add(newKvp);
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveKey(Type type, string relativePath, ValueDropdownList<int> values, IKey keyToRemove)
        {
            List<KeyValuePair> kvps = new();
            KeyValuePair removeKvp = new(keyToRemove, false);
            foreach (var value in values)
            {
                if (value.Value != removeKvp.key)
                {
                    kvps.Add(new(value.Text, value.Value, value.Text, false));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveKey(Type type, string relativePath, ValueDropdownList<string> values, IKey keyToRemove)
        {
            List<KeyValuePair> kvps = new();
            KeyValuePair removeKvp = new(keyToRemove, true);
            foreach (var value in values)
            {
                if (value.Value != removeKvp.internalID)
                {
                    kvps.Add(new(value.Text, value.Value.GetHashCode(), value.Text, false));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveKey(Type type, string relativePath, ValueDropdownList<int> values, string keyToRemove)
        {
            List<KeyValuePair> kvps = new();
            KeyValuePair removeKvp = StringToKeyValuePair(keyToRemove, false);
            foreach (var value in values)
            {
                if (value.Value != removeKvp.key)
                {
                    kvps.Add(new(value.Text, value.Value, value.Text, false));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveKey(Type type, string relativePath, ValueDropdownList<string> values, string keyToRemove)
        {
            List<KeyValuePair> kvps = new();
            KeyValuePair removeKvp = StringToKeyValuePair(keyToRemove, true);
            foreach (var value in values)
            {
                if (value.Value != removeKvp.internalID)
                {
                    kvps.Add(new(value.Text, value.Value.GetHashCode(), value.Text, false));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveKeys(Type type, string relativePath, ValueDropdownList<int> values, IKey[] keysToRemove)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> removeKvps = new();
            foreach (var keyToAdd in keysToRemove)
            {
                removeKvps.Add(new(keyToAdd, false));
            }
            foreach (var value in values)
            {
                if (!removeKvps.Exists(x => x.key == value.Value))
                {
                    kvps.Add(new(value.Text, value.Value, value.Text, false));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveKeys(Type type, string relativePath, ValueDropdownList<string> values, IKey[] keysToRemove)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> removeKvps = new();
            foreach (var keyToAdd in keysToRemove)
            {
                removeKvps.Add(new(keyToAdd, true));
            }
            foreach (var value in values)
            {
                if (!removeKvps.Exists(x => x.internalID == value.Value))
                {
                    kvps.Add(new(value.Text, value.Value.GetHashCode(), value.Text, false));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveKeys(Type type, string relativePath, ValueDropdownList<int> values, string[] keysToRemove)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> removeKvps = new();
            foreach (var keyToAdd in keysToRemove)
            {
                removeKvps.Add(StringToKeyValuePair(keyToAdd, false));
            }
            foreach (var value in values)
            {
                if (!removeKvps.Exists(x => x.key == value.Value))
                {
                    kvps.Add(new(value.Text, value.Value, value.Text, false));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveKeys(Type type, string relativePath, ValueDropdownList<string> values, string[] keysToRemove)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> removeKvps = new();
            foreach (var keyToAdd in keysToRemove)
            {
                removeKvps.Add(StringToKeyValuePair(keyToAdd, true));
            }
            foreach (var value in values)
            {
                if (!removeKvps.Exists(x => x.internalID == value.Value))
                {
                    kvps.Add(new(value.Text, value.Value.GetHashCode(), value.Text, false));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveDeprecated(Type type, string relativePath, ValueDropdownList<int> values, IKey[] keysToRemove)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> removeKvps = new();
            foreach (var keyToRemove in keysToRemove)
            {
                if (keyToRemove.IsDeprecated)
                {
                    removeKvps.Add(new(keyToRemove, false));
                }
            }
            foreach (var value in values)
            {
                if (!removeKvps.Exists(x => x.key == value.Value))
                {
                    kvps.Add(new(value.Text, value.Value, value.Text, false));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void RemoveDeprecated(Type type, string relativePath, ValueDropdownList<string> values, IKey[] keysToRemove)
        {
            List<KeyValuePair> kvps = new();
            List<KeyValuePair> removeKvps = new();
            foreach (var keyToRemove in keysToRemove)
            {
                if (keyToRemove.IsDeprecated)
                {
                    removeKvps.Add(new(keyToRemove, true));
                }
            }
            foreach (var value in values)
            {
                if (!removeKvps.Exists(x => x.internalID == value.Value))
                {
                    kvps.Add(new(value.Text, value.Value.GetHashCode(), value.Text, false));
                }
            }

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }
#endif

#if !ODIN_INSPECTOR
        public static void AddKey(Type type, string relativePath, List<(string, int)> values, string name, int key, string internalID)
        {
            List<KeyValuePair> kvps = new();
            foreach (var value in values)
            {
                kvps.Add(new(value.Item1, value.Item2, value.Item1, false));
            }
            kvps.Add(new(name, key, internalID, false));

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }

        public static void AddKey(Type type, string relativePath, List<(string, string)> values, string name, int key, string internalID)
        {
            List<KeyValuePair> kvps = new();
            foreach (var value in values)
            {
                kvps.Add(new(value.Item1, value.Item2.GetHashCode(), value.Item2, true));
            }
            kvps.Add(new(name, key, internalID, true));

            FormatKeyFiles(relativePath, type.Namespace, type.Name, kvps);
        }
#endif

        private static IEnumerable<T> GetAllAssets<T>(string[] assetPaths) where T : Object
        {
            foreach (var path in assetPaths)
            {
                yield return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(path));
            }
        }
#endif

        public static void FormatKeyFiles(string gameDataFilepath, string namespaceName, string scriptName, List<KeyValuePair> keys)
        {
            string gameDataProjectFilePath = $"/{gameDataFilepath}/{scriptName}.cs";
            string filepath = Application.dataPath + gameDataProjectFilePath;

            StringBuilder sb = new();


            sb.Append("//\t* THIS SCRIPT IS AUTO-GENERATED *\n");
            sb.Append("#if ODIN_INSPECTOR\n using Sirenix.OdinInspector;\n #endif\n");
            sb.Append("using System;\n");
            sb.Append("using System.Collections.Generic;\n\n");

            sb.Append($"namespace {namespaceName}\n");
            sb.Append("{\n");
            sb.Append($"\tpublic class {scriptName}\n");
            sb.Append("\t{\n");

            bool useInternalID = false;
            if (keys.Count > 0)
            {
                useInternalID = keys[0].useInternalID;
            }

            FormatFilePath(sb, $"{gameDataFilepath}", useInternalID);

            if (!useInternalID)
            {
                FormatEnum(sb, keys);
            }
            FormatOdinDropDown(sb, keys, useInternalID);

            for (int i = 0; i < keys.Count; i++)
            {
                int pIndex = i;
                sb.Append("\t\t");
                sb.Append(keys[i].GetFormat(pIndex));
                sb.Append("\n");
            }

            FormatList(sb, keys);
            FormatLookup(sb, useInternalID);

            sb.Append("\t}\n");
            sb.Append("}");

            System.IO.File.WriteAllText(filepath, sb.ToString());
        }

        private static void FormatFilePath(StringBuilder sb, string relativePath, bool useInternalID)
        {
            sb.Append($"\t\tpublic const string RELATIVE_PATH = \"{relativePath}\";\n");
            if (useInternalID)
            {
                sb.Append($"\t\tpublic const bool USING_INTERNAL_ID = true;\n");
            }
            else
            {
                sb.Append($"\t\tpublic const bool USING_INTERNAL_ID = false;\n");
            }
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

        private static void FormatOdinDropDown(StringBuilder sb, List<KeyValuePair> keys, bool useInternalID)
        {
            sb.Append("#if ODIN_INSPECTOR\n");
            if (useInternalID)
            {
                sb.Append($"\t\tpublic static ValueDropdownList<string> DropdownValues = new()\n");
            }
            else
            {
                sb.Append($"\t\tpublic static ValueDropdownList<int> DropdownValues = new()\n");
            }
            sb.Append("\t\t{\n");
            for (int i = 0; i < keys.Count; i++)
            {
                sb.Append($"\t\t\t{{ \"{keys[i].displayName}\", {keys[i].variableName} }},\n");
            }
            sb.Append("\t\t};\n");
            sb.Append("#endif\n\n");

            sb.Append("#if !ODIN_INSPECTOR\n");
            if (useInternalID)
            {
                sb.Append($"\t\tpublic static List<(string, string)> DropdownValues = new()\n");
            }
            else
            {
                sb.Append($"\t\tpublic static List<(string, int)> DropdownValues = new()\n");
            }
            sb.Append("\t\t{\n");
            for (int i = 0; i < keys.Count; i++)
            {
                sb.Append($"\t\t\tnew (\"{keys[i].displayName}\", {keys[i].variableName}),\n");
            }
            sb.Append("\t\t};\n");
            sb.Append("#endif\n\n");
        }

        private static void FormatList(StringBuilder sb, List<KeyValuePair> keys)
        {
            sb.Append("\n");
            sb.Append($"\t\tpublic static List<int> Values = new()\n");
            sb.Append("\t\t{\n");
            for (int i = 0; i < keys.Count; i++)
            {
                sb.Append($"\t\t\t{{ {keys[i].variableName} }},\n");
            }
            sb.Append("\t\t};\n");
        }

        private static void FormatEnumeration(StringBuilder sb, string scriptName, bool useInternalID)
        {
            if (useInternalID)
            {
                sb.Append($"\t\tpublic static IEnumerable<string> EnumerateValues()\n");
                sb.Append("\t\t{\n");

                sb.Append($"\t\t\tvar refVals = (typeof({scriptName})).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string));\n");
                sb.Append("\n");
                sb.Append($"\t\t\tforeach (var fi in refVals)\n");
                sb.Append("\t\t\t{\n");
                sb.Append($"\t\t\t\tyield return (string)fi.GetRawConstantValue();\n");
                sb.Append("\t\t\t}\n");

                sb.Append("\t\t}\n");
            }
            else
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
        }

        private static void FormatLookup(StringBuilder sb, bool useInternalID)
        {
            sb.Append("#if ODIN_INSPECTOR\n");
            if (useInternalID)
            {
                sb.Append($"\t\tpublic static string Lookup(string id)\n");
                sb.Append("\t\t{\n");

                sb.Append($"\t\t\treturn DropdownValues.Find((x) => x.Value == id).Text;\n");

                sb.Append("\t\t}\n");
            }
            else
            {
                sb.Append($"\t\tpublic static string Lookup(int id)\n");
                sb.Append("\t\t{\n");

                sb.Append($"\t\t\treturn DropdownValues.Find((x) => x.Value == id).Text;\n");

                sb.Append("\t\t}\n");
            }
            sb.Append("#endif\n");

            sb.Append("#if !ODIN_INSPECTOR\n");
            if (useInternalID)
            {
                sb.Append($"\t\tpublic static string Lookup(string id)\n");
                sb.Append("\t\t{\n");

                sb.Append($"\t\t\treturn DropdownValues.Find((x) => x.Item2 == id).Item1;\n");

                sb.Append("\t\t}\n");
            }
            else
            {
                sb.Append($"\t\tpublic static string Lookup(int id)\n");
                sb.Append("\t\t{\n");

                sb.Append($"\t\t\treturn DropdownValues.Find((x) => x.Item2 == id).Item1;\n");

                sb.Append("\t\t}\n");
            }
            sb.Append("#endif\n");
        }
        #endregion
    }
}
