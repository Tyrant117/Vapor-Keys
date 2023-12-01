using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VaporKeys.UIElements;

namespace VaporKeys.Editor
{
    [CustomPropertyDrawer(typeof(KeyDropdownValue))]
    public class KeyDropdownValuePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var outerBox = new VisualElement();
            var attr = fieldInfo.GetCustomAttribute<KeyDropdownAttribute>(true);
            bool boundToKeys = false;
            bool isArray = false;
            var split = property.propertyPath.Split(new char[] { '.' });
            if (split.Length > 0)
            {
                var arrProp = property.serializedObject.FindProperty(split[0]);
                isArray = arrProp.isArray;
            }
            if (attr != null)
            {
                boundToKeys = true;
                List<(string, KeyDropdownValue)> dropdownList = GetKeys(property, attr.ValuesName);
                if (dropdownList != null)
                {
                    outerBox.Add(_DrawDropdown(dropdownList));
                }
            }

            var guidBox = new VisualElement();
            guidBox.style.flexDirection = FlexDirection.Row;
            guidBox.style.justifyContent = Justify.SpaceBetween;
            var guidLabel = new Label("Guid")
            {
                name = "guid-label"
            };
            guidLabel.style.width = 70;
            guidLabel.style.display = boundToKeys ? _GetDisplayStyle() : DisplayStyle.Flex;
            guidBox.Add(guidLabel);
            var guidField = new PropertyField(property.FindPropertyRelative("Guid"), "")
            {
                name = "guid-prop"
            };
            guidField.style.display = boundToKeys ? _GetDisplayStyle() : DisplayStyle.Flex;
            guidField.style.flexGrow = 1;
            guidField.SetEnabled(false);
            guidBox.Add(guidField);
            var selectButton = new Button(() => OnSelectClicked(property))
            {
                name = "select-button",
                text = "Select"
            };
            selectButton.style.display = boundToKeys ? _GetDisplayStyle() : DisplayStyle.Flex;
            selectButton.style.width = 64;
            guidBox.Add(selectButton);

            var keyBox = new VisualElement();
            keyBox.style.flexDirection = FlexDirection.Row;
            keyBox.style.justifyContent = Justify.SpaceBetween;
            var keyLabel = new Label("Key")
            {
                name = "key-label"
            };
            keyLabel.style.width = 70;
            keyLabel.style.display = boundToKeys ? _GetDisplayStyle() : DisplayStyle.Flex;
            var keyField = new PropertyField(property.FindPropertyRelative("Key"), "")
            {
                name = "key-prop"
            };
            keyField.style.display = boundToKeys ? _GetDisplayStyle() : DisplayStyle.Flex;
            keyField.style.flexGrow = 1;
            keyField.SetEnabled(!boundToKeys);
            keyBox.Add(keyLabel);
            keyBox.Add(keyField);
            var remapButton = new Button(() => OnRemapClicked(property))
            {
                name = "remap-button",
                text = "Re-map"
            };
            remapButton.style.display = boundToKeys ? _GetDisplayStyle() : DisplayStyle.Flex;
            remapButton.style.width = 64;
            keyBox.Add(remapButton);

            outerBox.Add(guidBox);
            outerBox.Add(keyBox);

            return outerBox;

            VisualElement _DrawDropdown(List<(string, KeyDropdownValue)> dropdownList)
            {
                // Current value index, values and display options
                int index = -1;
                int selectedValueIndex = -1;
                List<string> displayOptions = new();

                using (var dropdownEnumerator = dropdownList.GetEnumerator())
                {
                    while (dropdownEnumerator.MoveNext())
                    {
                        index++;

                        var current = dropdownEnumerator.Current;
                        if (current.Item2.Equals(property.boxedValue))
                        {
                            selectedValueIndex = index;
                        }

                        if (current.Item1 == null)
                        {
                            displayOptions.Add("<null>");
                        }
                        else if (string.IsNullOrWhiteSpace(current.Item1))
                        {
                            displayOptions.Add("<empty>");
                        }
                        else
                        {
                            displayOptions.Add(current.Item1);
                        }
                    }
                }

                if (selectedValueIndex < 0)
                {
                    selectedValueIndex = 0;
                }

                var dropdownBox = new VisualElement();
                dropdownBox.style.flexDirection = FlexDirection.Row;

                var tog = new Toggle();
                tog.SetValueWithoutNotify(property.isExpanded);
                tog.RegisterValueChangedCallback(x => OnViewKeyPropertiesChanged(property, outerBox, x));

                var search = new SearchableTextField(isArray ? "" : property.displayName, displayOptions, selectedValueIndex);
                search.style.flexGrow = 1;
                search.style.minWidth = 120;
                search.style.marginLeft = isArray ? 58 : 6;
                search.SearchChanged += x => OnSearchChanged(property, search, dropdownList, x);
                //search.EndEdit += () => OnEndEditLock(search);

                //var lockButton = new Button(() => OnToggleReadonlyLock(search));
                //lockButton.style.width = 20;
                //lockButton.style.height = 18;
                //lockButton.style.marginLeft = 6;
                //lockButton.style.paddingBottom = 1;
                //lockButton.style.paddingLeft = 1;
                //lockButton.style.paddingRight = 1;
                //lockButton.style.paddingTop = 1;
                //lockButton.style.backgroundImage = new StyleBackground((Texture2D)EditorGUIUtility.IconContent("d_InspectorLock").image);
                //lockButton.style.backgroundSize = new StyleBackgroundSize(new BackgroundSize(16, 14));

                dropdownBox.Add(tog);
                dropdownBox.Add(search);
                //dropdownBox.Add(lockButton);
                return dropdownBox;
            }

            DisplayStyle _GetDisplayStyle() => property.isExpanded ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnViewKeyPropertiesChanged(SerializedProperty property, VisualElement container, ChangeEvent<bool> evt)
        {
            //Debug.Log(property.propertyPath);
            property.isExpanded = evt.newValue;
            if (evt.newValue)
            {
                container.Q<Label>("guid-label").style.display = DisplayStyle.Flex;
                container.Q<PropertyField>("guid-prop").style.display = DisplayStyle.Flex;
                container.Q<Button>("select-button").style.display = DisplayStyle.Flex;

                container.Q<Label>("key-label").style.display = DisplayStyle.Flex;
                container.Q<PropertyField>("key-prop").style.display = DisplayStyle.Flex;
                container.Q<Button>("remap-button").style.display = DisplayStyle.Flex;
            }
            else
            {
                container.Q<Label>("guid-label").style.display = DisplayStyle.None;
                container.Q<PropertyField>("guid-prop").style.display = DisplayStyle.None;
                container.Q<Button>("select-button").style.display = DisplayStyle.None;

                container.Q<Label>("key-label").style.display = DisplayStyle.None;
                container.Q<PropertyField>("key-prop").style.display = DisplayStyle.None;
                container.Q<Button>("remap-button").style.display = DisplayStyle.None;
            }
            evt.StopPropagation();
        }

        private void OnSearchChanged(SerializedProperty property, SearchableTextField dropdown, List<(string, KeyDropdownValue)> list, string evt)
        {
            if (property != null && list.Exists(x => x.Item1 == evt))
            {
                property.boxedValue = list.Find(x => x.Item1 == evt).Item2;
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnSelectClicked(SerializedProperty property)
        {
            if (property != null && property.boxedValue is KeyDropdownValue key)
            {
                key.Select();
            }
        }

        private void OnRemapClicked(SerializedProperty property)
        {
            if (property != null && property.boxedValue is KeyDropdownValue key)
            {
                key.Remap();
            }
        }

        private List<(string, KeyDropdownValue)> GetKeys(SerializedProperty property, string valuesName)
        {
            var type = Type.GetType($"VaporKeyDefinitions.{valuesName}, VaporKeyDefinitions, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            var fieldInfo = type.GetField("DropdownValues", BindingFlags.Static | BindingFlags.Public);
            if (fieldInfo != null)
            {
                var keys = fieldInfo.GetValue(null);
                if (keys is List<(string, KeyDropdownValue)> keyList)
                {
                    return keyList;
                }
            }
            return null;
        }
    }
}
