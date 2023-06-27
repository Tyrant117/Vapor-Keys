#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using System.Diagnostics;
using UnityEngine;

namespace VaporKeys
{
    [System.Serializable]
    public struct KeyDropdownValue
    {
        public string Guid;
#if ODIN_INSPECTOR
        [InlineButton("Remap")]
#endif
        public int Key;

        public KeyDropdownValue(string guid, int key)
        {
            Guid = guid;
            Key = key;
        }

        public static KeyDropdownValue None => new (string.Empty, 0);

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
                }
            }
#endif
        }
    }
}
