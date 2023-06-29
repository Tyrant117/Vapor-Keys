#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;

namespace VaporKeys
{
    public abstract class ScriptableObjectKey : ScriptableObject, IKey
    {
#if ODIN_INSPECTOR
        [ReadOnly]
#endif
        [SerializeField]
        private int _key;
        public int Key => _key;
        public void ForceRefreshKey() { _key = name.GetKeyHashCode(); }
        public abstract string DisplayName { get; }
        public bool IsDeprecated => _deprecated;

        [SerializeField]
        protected bool _deprecated;
    }
}
