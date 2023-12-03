using UnityEngine;

namespace VaporKeys
{
    public abstract class KeySO : ScriptableObject, IKey
    {
        [SerializeField]
        private int _key;
        public int Key => _key;
        public void ForceRefreshKey() { _key = name.GetKeyHashCode(); }
        public abstract string DisplayName { get; }
        public bool IsDeprecated => _deprecated;

        [SerializeField]
        [Tooltip("If TRUE, this key will be ignored by KeyGenerator.GenerateKeys().")]
        protected bool _deprecated;

        public virtual void GenerateKeys()
        {
            var scriptName = GetType().Name;
            scriptName = scriptName.Replace("Scriptable", "");
            scriptName = scriptName.Replace("SO", "");
            KeyGenerator.GenerateKeys(GetType(), $"{scriptName}Keys", true);
        }
    }
}
