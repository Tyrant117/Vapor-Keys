#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;

namespace VaporKeys
{
    public abstract class OdinKeySO : ScriptableObject, IKey
    {
#if ODIN_INSPECTOR
        [ReadOnly]
        [FoldoutGroup("Key Data")]
#endif
        [SerializeField]
        private int _key;
#if ODIN_INSPECTOR
        [FoldoutGroup("Key Data")]
#endif
        [SerializeField]
        [Tooltip("If TRUE, this key will be ignored by KeyGenerator.GenerateKeys().")]
        protected bool _deprecated;

        public int Key => _key;
        public void ForceRefreshKey() { _key = name.GetKeyHashCode(); }
        public abstract string DisplayName { get; }
        public bool IsDeprecated => _deprecated;


#if ODIN_INSPECTOR
        [FoldoutGroup("Key Data")]
        [Button]
#endif
        public virtual void GenerateKeys()
        {
            var scriptName = GetType().Name;
            scriptName = scriptName.Replace("Scriptable", "");
            scriptName = scriptName.Replace("SO", "");
            KeyGenerator.GenerateKeys(GetType(), $"{scriptName}Keys", true);
        }
    }
}
