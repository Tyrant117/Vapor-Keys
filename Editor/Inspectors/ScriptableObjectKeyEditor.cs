using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace VaporKeys.Editor
{
    [CustomEditor(typeof(ScriptableObjectKey), true)]
    public class ScriptableObjectKeyEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            DrawScript(container);
            DrawKey(container);
            DrawUIElementsPropertiesExcluding(container, "_key");
            return container;
        }

        protected void DrawScript(VisualElement container)
        {
            var script = new PropertyField(serializedObject.FindProperty("m_Script"));
            script.SetEnabled(false);
            container.Add(script);
        }

        protected void DrawKey(VisualElement container)
        {
            var keyField = new PropertyField(serializedObject.FindProperty("_key"));
            keyField.SetEnabled(false);
            container.Add(keyField);
        }

        protected void DrawUIElementsPropertiesExcluding(VisualElement container, params string[] propertyToExclude)
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.name.Equals("m_Script"))
                {
                    continue;
                }
                if (!propertyToExclude.Contains(iterator.name))
                {
                    container.Add(new PropertyField(iterator));
                }
            }
        }
    }
}
