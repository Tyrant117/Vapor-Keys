#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System;
using System.Diagnostics;
using UnityEngine;

namespace VaporKeys
{
    [System.Serializable]
    public struct KeyDropdownValueOdin : IEquatable<KeyDropdownValueOdin>
    {
        public static implicit operator int(KeyDropdownValueOdin kdv) => kdv.Key;

#if ODIN_INSPECTOR
        [InlineButton("Select")]
#endif
        public string Guid;
#if ODIN_INSPECTOR
        [InlineButton("Remap")]
#endif
        public int Key;

        public KeyDropdownValueOdin(string guid, int key)
        {
            Guid = guid;
            Key = key;
        }

        public static KeyDropdownValueOdin None => new(string.Empty, 0);

        [Conditional("UNITY_EDITOR")]
        public void Select()
        {
#if UNITY_EDITOR
            if (Guid != string.Empty)
            {
                var refVal = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(Guid));
                UnityEditor.Selection.activeObject = refVal;
            }
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public void Remap()
        {
#if UNITY_EDITOR
            if (Guid != string.Empty)
            {
                var refVal = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(UnityEditor.AssetDatabase.GUIDToAssetPath(Guid));
                if (refVal is IKey rfk)
                {
                    rfk.ForceRefreshKey();
                    Key = rfk.Key;
                    UnityEditor.EditorUtility.SetDirty(refVal);
                }
            }
#endif
        }

        public override string ToString()
        {
            return $"Key: {Key} belonging to {Guid}";
        }

        public override bool Equals(object obj)
        {
            return obj is KeyDropdownValueOdin other && Equals(other);
        }

        public bool Equals(KeyDropdownValueOdin other)
        {
            return Guid.Equals(other.Guid) && Key.Equals(other.Key);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Guid, Key);
        }
    }
}
