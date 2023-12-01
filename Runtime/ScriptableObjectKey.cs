using UnityEngine;

namespace VaporKeys
{
    public abstract class ScriptableObjectKey : ScriptableObject, IKey
    {
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
