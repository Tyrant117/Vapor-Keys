using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace VaporKeys.Editor
{
    [CustomEditor(typeof(KeySO), true)]
    public class KeySOEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            DrawScript(container);
            DrawKeyFoldout(container);            
            DrawUIElementsPropertiesExcluding(container, "_key", "m_Script", "_deprecated");
            return container;
        }

        private void OnGenerateKeysClicked()
        {
            var target = (KeySO)serializedObject.targetObject;
            target.GenerateKeys();
        }

        protected void DrawScript(VisualElement container)
        {
            var script = new PropertyField(serializedObject.FindProperty("m_Script"));
            script.SetEnabled(false);
            container.Add(script);
        }

        protected void DrawKeyFoldout(VisualElement container)
        {
            

            var deprecated = serializedObject.FindProperty("_deprecated").boolValue;
            var foldout = new StyledFoldout(deprecated ? "Key Data (Deprecated)" : "Key Data");
            foldout.Add(DrawKey());
            foldout.Add(DrawIsDeprecated());
            var generateButton = new Button(OnGenerateKeysClicked)
            {
                text = "Generate Keys"
            };
            foldout.Add(generateButton);
            container.Add(foldout);

            //var box = new Box();
            //box.style.borderBottomColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            //box.style.borderTopColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            //box.style.borderRightColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            //box.style.borderLeftColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            //box.style.borderBottomLeftRadius = 3;
            //box.style.borderBottomRightRadius = 3;
            //box.style.borderTopLeftRadius = 3;
            //box.style.borderTopRightRadius = 3;
            //box.style.marginTop = 3;
            //box.style.marginBottom = 3;
            //box.style.marginLeft = 0;
            //box.style.marginRight = 0;
            //box.style.backgroundColor = new StyleColor(new Color(0.2509804f, 0.2509804f, 0.2509804f));
            //var foldout = new Foldout()
            //{
            //    text = deprecated ? "Key Data (Deprecated)" : "Key Data"
            //};
            //var togStyle = foldout.Q<Toggle>().style;
            //togStyle.marginTop = 0;
            //togStyle.marginLeft = 0;
            //togStyle.marginRight = 0;
            //togStyle.marginBottom = 0;
            //togStyle.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
            //foldout.Q<Toggle>().hierarchy[0].style.marginLeft = 3;
            //foldout.Q<Toggle>().hierarchy[0].style.marginTop = 3;
            //foldout.Q<Toggle>().hierarchy[0].style.marginBottom = 3;
            //foldout.Q<Toggle>().Q<Label>().style.unityFontStyleAndWeight = FontStyle.Bold;
            //var contentStyle = foldout.Q<VisualElement>("unity-content").style;
            //contentStyle.marginTop = 3;
            //contentStyle.marginRight = 4;
            //contentStyle.marginBottom = 3;
            //contentStyle.marginLeft = 3;
            //foldout.value = false;

            //DrawKey(foldout);
            //DrawIsDeprecated(foldout);
            //var generateButton = new Button(OnGenerateKeysClicked)
            //{
            //    text = "Generate Keys"
            //};
            //foldout.Add(generateButton);
            //box.Add(foldout);
            //container.Add(box);
        }

        protected VisualElement DrawKey()
        {
            var field = new PropertyField(serializedObject.FindProperty("_key"));
            field.SetEnabled(false);
            return field;
        }

        protected VisualElement DrawIsDeprecated()
        {
            var field = new PropertyField(serializedObject.FindProperty("_deprecated"));
            return field;
        }

        protected void DrawUIElementsPropertiesExcluding(VisualElement container, params string[] propertyToExclude)
        {
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (!propertyToExclude.Contains(iterator.name))
                {
                    container.Add(new PropertyField(iterator));
                }
            }
        }

        //public override void OnInspectorGUI()
        //{
        //    serializedObject.Update();
        //    DrawScript();
        //    DrawKey();
        //    DrawPropertiesExcluding("_key", "m_Script");
        //    if (GUILayout.Button("Generate Keys"))
        //    {
        //        var target = (KeySO)serializedObject.targetObject;
        //        target.GenerateKeys();
        //    }
        //    serializedObject.ApplyModifiedProperties();
        //}        

        //protected void DrawScript()
        //{
        //    using (new EditorGUI.DisabledScope(true))
        //    {
        //        if (target is MonoBehaviour behaviour)
        //        {
        //            EditorGUILayout.ObjectField(EditorGUIUtility.TrTempContent("Script"), MonoScript.FromMonoBehaviour(behaviour), typeof(MonoBehaviour), false);
        //        }
        //        else if (target is ScriptableObject scriptableObject)
        //        {
        //            EditorGUILayout.ObjectField(EditorGUIUtility.TrTempContent("Script"), MonoScript.FromScriptableObject(scriptableObject), typeof(ScriptableObject), false);
        //        }
        //    }
        //}

        //protected void DrawKey()
        //{
        //    using (new EditorGUI.DisabledScope(true))
        //    {
        //        EditorGUILayout.PropertyField(serializedObject.FindProperty("_key"), EditorGUIUtility.TrTempContent("Key"));
        //    }
        //}

        //protected void DrawPropertiesExcluding(params string[] propertyToExclude)
        //{
        //    SerializedProperty iterator = serializedObject.GetIterator();
        //    bool enterChildren = true;
        //    while (iterator.NextVisible(enterChildren))
        //    {
        //        enterChildren = false;
        //        if (!propertyToExclude.Contains(iterator.name))
        //        {
        //            EditorGUILayout.PropertyField(iterator);
        //        }
        //    }
        //}
    }
}
