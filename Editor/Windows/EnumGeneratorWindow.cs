using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ObjectField = UnityEditor.UIElements.ObjectField;

namespace VaporKeys.Editor
{
    public class EnumGeneratorWindow : EditorWindow
    {
        public enum OrderDirection
        {
            CountUp,
            CountDown
        }

        private const string configPath = "Assets/Vapor Keys/Config";
        private const string keyPath = "Vapor Keys/Keys";

        private ListView _list;
        private List<BackingContext> _enums = new();
        private List<BackingContext> _filteredEnums = new();
        private TextField _selected;

        public class BackingContext
        {
            public bool toggled;
            public string value;

            public BackingContext()
            {
                toggled = false;
                value = "";
            }

            public BackingContext(bool toggled, string value)
            {
                this.toggled = toggled;
                this.value = value;
            }
        }

        [MenuItem("Tools/Vapor/Enum Generator")]
        public static void ShowWindow()
        {
            // This method is called when the user selects the menu item in the Editor
            EditorWindow wnd = GetWindow<EnumGeneratorWindow>();
            wnd.titleContent = new GUIContent("Enum Generator");

            // Limit size of the window
            wnd.minSize = new Vector2(500, 200);
            wnd.maxSize = new Vector2(1920, 720);
        }

        public void CreateGUI()
        {
            var loadingBox = new Box();
            loadingBox.style.marginTop = 3;
            loadingBox.style.marginBottom = 3;
            loadingBox.style.marginLeft = 3;
            loadingBox.style.marginRight = 3;
            loadingBox.style.borderTopWidth = 1f;
            loadingBox.style.borderBottomWidth = 1f;
            loadingBox.style.borderLeftWidth = 1f;
            loadingBox.style.borderRightWidth = 1f;
            loadingBox.style.borderTopColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            loadingBox.style.borderBottomColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            loadingBox.style.borderLeftColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            loadingBox.style.borderRightColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            loadingBox.style.borderTopLeftRadius = 3;
            loadingBox.style.borderTopRightRadius = 3;
            loadingBox.style.borderBottomLeftRadius = 3;
            loadingBox.style.borderBottomRightRadius = 3;
            loadingBox.style.paddingBottom = 3;
            loadingBox.style.backgroundColor = new StyleColor(new Color(0.2509804f, 0.2509804f, 0.2509804f));

            var loadingHeader = new Label("Loading");
            loadingHeader.style.paddingLeft = 6;
            loadingHeader.style.paddingTop = 3;
            loadingHeader.style.paddingBottom = 3;
            loadingHeader.style.borderBottomWidth = 1f;
            loadingHeader.style.borderBottomColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            loadingHeader.style.marginBottom = 6;
            loadingHeader.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));

            loadingBox.Add(loadingHeader);
            var currentEnumGUIDs = AssetDatabase.FindAssets("t:TextAsset", new string[] { configPath });
            List<string> choices = new();
            var dropdownField = new DropdownField("Class", choices, 0) { name = "load-dropdown" };
            foreach (var guid in currentEnumGUIDs)
            {
                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guid));
                choices.Add(textAsset.name);
            }
            dropdownField.index = 0;

            loadingBox.Add(dropdownField);
            //loadingBox.Add(new ObjectField() { name = "load-object", objectType = typeof(TextAsset) });
            loadingBox.Add(new Button(OnLoad) { text = "Load" });
            rootVisualElement.Add(loadingBox);

            var configBox = new Box();
            configBox.style.marginTop = 3;
            configBox.style.marginBottom = 3;
            configBox.style.marginLeft = 3;
            configBox.style.marginRight = 3;
            configBox.style.borderTopWidth = 1f;
            configBox.style.borderBottomWidth = 1f;
            configBox.style.borderLeftWidth = 1f;
            configBox.style.borderRightWidth = 1f;
            configBox.style.borderTopColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            configBox.style.borderBottomColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            configBox.style.borderLeftColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            configBox.style.borderRightColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            configBox.style.borderTopLeftRadius = 3;
            configBox.style.borderTopRightRadius = 3;
            configBox.style.borderBottomLeftRadius = 3;
            configBox.style.borderBottomRightRadius = 3;
            configBox.style.paddingBottom = 3;
            configBox.style.backgroundColor = new StyleColor(new Color(0.2509804f, 0.2509804f, 0.2509804f));

            var configHeader = new Label("Config");
            configHeader.style.paddingLeft = 6;
            configHeader.style.paddingTop = 3;
            configHeader.style.paddingBottom = 3;
            configHeader.style.borderBottomWidth = 1f;
            configHeader.style.borderBottomColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            configHeader.style.marginBottom = 6;
            configHeader.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));

            configBox.Add(configHeader);
            configBox.Add(new TextField("Class Name") { name = "class-name-text"});
            configBox.Add(new Toggle("Create None") { name = "create-none-toggle", value = true });
            var customOrderToggle = new Toggle("Custom Order") { name = "custom-order-toggle" };           
            customOrderToggle.RegisterValueChangedCallback(OnCustomOrderToggled);
            configBox.Add(customOrderToggle);
            configBox.Add(new IntegerField("Starting Value") { name = "starting-value-int" });
            configBox.Add(new EnumField("Order Direction", OrderDirection.CountUp) { name = "order-direction-enum" });                        

            var box = new Box();
            box.style.marginTop = 3;
            box.style.marginBottom = 3;
            box.style.marginLeft = 3;
            box.style.marginRight = 3;
            box.style.borderTopWidth = 1f;
            box.style.borderBottomWidth = 1f;
            box.style.borderLeftWidth = 1f;
            box.style.borderRightWidth = 1f;
            box.style.borderTopColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            box.style.borderBottomColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            box.style.borderLeftColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            box.style.borderRightColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            box.style.borderTopLeftRadius = 3;
            box.style.borderTopRightRadius = 3;
            box.style.borderBottomLeftRadius = 3;
            box.style.borderBottomRightRadius = 3;
            var horizontal = new VisualElement();
            horizontal.style.flexDirection = FlexDirection.Row;
            horizontal.style.borderBottomWidth = 1f;
            horizontal.style.borderBottomColor = new StyleColor(new Color(0.132f, 0.132f, 0.132f));
            var foldout = new Foldout()
            {
                text = "Enum Content"
            };
            foldout.style.flexGrow = 1;
            foldout.RegisterValueChangedCallback(x => OnToggleListView(x));
            var scrollView = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "enum-scroll"
            };
            scrollView.style.display = foldout.value ? DisplayStyle.Flex : DisplayStyle.None;
            scrollView.style.maxHeight = 200f;


            horizontal.Add(foldout);
            var searchBar = new ToolbarSearchField() { name = "search-field" };
            searchBar.RegisterValueChangedCallback(OnSearchChanged);
            horizontal.Add(searchBar);
            var rb = new Button(RemoveEnumItem) { text = "-" };
            rb.style.width = 20;
            rb.style.fontSize = 14;
            horizontal.Add(rb);
            var ab = new Button(AddEnumItem) { text = "+" };
            ab.style.width = 20;
            ab.style.fontSize = 14;
            horizontal.Add(ab);

            box.Add(horizontal);
            box.Add(scrollView);

            configBox.Add(box);
            configBox.Add(new Button(OnCreate) { text = "Create" });
            rootVisualElement.Add(configBox);
            OnCustomOrderToggled(ChangeEvent<bool>.GetPooled(false, false));
        }

        private void OnCreate()
        {
            var definitionName = rootVisualElement.Q<TextField>("class-name-text").value;
            var createNone = rootVisualElement.Q<Toggle>("create-none-toggle").value;
            var customOrder = rootVisualElement.Q<Toggle>("custom-order-toggle").value;
            var startingValue = rootVisualElement.Q<IntegerField>("starting-value-int").value;
            var orderDirection = (OrderDirection)rootVisualElement.Q<EnumField>("order-direction-enum").value;

            var kvp = new List<KeyGenerator.KeyValuePair>();
            int currentValue = startingValue;
            if (createNone)
            {
                if (customOrder)
                {
                    kvp.Add(new KeyGenerator.KeyValuePair("None", currentValue, string.Empty));
                    switch (orderDirection)
                    {
                        case OrderDirection.CountUp:
                            currentValue++;
                            break;
                        case OrderDirection.CountDown:
                            currentValue--;
                            break;
                    }
                }
                else
                {
                    kvp.Add(new KeyGenerator.KeyValuePair("None", 0, string.Empty));
                }
            }
            foreach (var name in _enums)
            {
                if (customOrder)
                {
                    kvp.Add(new KeyGenerator.KeyValuePair(name.value, currentValue, string.Empty));
                    switch (orderDirection)
                    {
                        case OrderDirection.CountUp:
                            currentValue++;
                            break;
                        case OrderDirection.CountDown:
                            currentValue--;
                            break;
                    }
                }
                else
                {
                    kvp.Add(new KeyGenerator.KeyValuePair(name.value, name.value.GetKeyHashCode(), string.Empty));
                }
            }

            KeyGenerator.FormatKeyFiles(keyPath, "VaporKeyDefinitions", definitionName, kvp);

            var jsonDef = new JSONConfigDefinition()
            {
                FolderPath = keyPath,
                NamespaceName = "VaporKeyDefinitions",
                DefinitionName = definitionName,
                CustomOrder = customOrder,
                StartingValue = startingValue,
                OrderDirection = (int)orderDirection,
                CreateNone = createNone,
                EnumContent = new List<string>()
            };
            foreach (var e in _enums)
            {
                jsonDef.EnumContent.Add(e.value);
            }

            JSONConfigDefinition.ToJson(jsonDef, $"{Application.dataPath}/{"Vapor Keys/Config"}/{definitionName}.json");

            //var def = ScriptableObject.CreateInstance<ScriptableDefinition>();
            //def.folderPath = keyPath;
            //def.namespaceName = "VaporKeyDefinitions";
            //def.definitionName = definitionName;
            //def.customOrder = customOrder;
            //def.startingValue = startingValue;
            //def.orderDirection = (int)orderDirection;
            //def.createNone = createNone;
            //def.enumContent = new List<string>();
            //foreach (var e in _enums)
            //{
            //    def.enumContent.Add(e.value);
            //}

            //AssetDatabase.CreateAsset(def, $"{configPath}/{definitionName}.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void OnLoad()
        {
            if (rootVisualElement.Q<DropdownField>("load-dropdown").value.Length > 0)
            {
                var targetName = rootVisualElement.Q<DropdownField>("load-dropdown").value;
                var currentEnumGUIDs = AssetDatabase.FindAssets("t:TextAsset", new string[] { configPath });
                foreach (var guid in currentEnumGUIDs)
                {
                    var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guid));
                    if (textAsset.name.Equals(targetName))
                    {
                        var definition = JSONConfigDefinition.FromJson($"{Application.dataPath}/Vapor Keys/Config", $"{targetName}.json");
                        rootVisualElement.Q<TextField>("class-name-text").value = definition.DefinitionName;
                        rootVisualElement.Q<Toggle>("create-none-toggle").value = definition.CreateNone;
                        rootVisualElement.Q<Toggle>("custom-order-toggle").value = definition.CustomOrder;
                        rootVisualElement.Q<IntegerField>("starting-value-int").value = definition.StartingValue;
                        rootVisualElement.Q<EnumField>("order-direction-enum").value = (OrderDirection)definition.OrderDirection;
                        _enums.Clear();
                        foreach (var e in definition.EnumContent)
                        {
                            _enums.Add(new BackingContext(false, e));
                        }
                        break;
                    }
                }
                Rebuild();
            }

            //if (rootVisualElement.Q<ObjectField>("load-object").value is ScriptableDefinition definition)
            //{
            //    rootVisualElement.Q<TextField>("class-name-text").value = definition.definitionName;
            //    rootVisualElement.Q<Toggle>("create-none-toggle").value = definition.createNone;
            //    rootVisualElement.Q<Toggle>("custom-order-toggle").value = definition.customOrder;
            //    rootVisualElement.Q<IntegerField>("starting-value-int").value = definition.startingValue;
            //    rootVisualElement.Q<EnumField>("order-direction-enum").value = (OrderDirection)definition.orderDirection;
            //    _enums.Clear();
            //    foreach (var e in definition.enumContent)
            //    {
            //        _enums.Add(new BackingContext(false, e));
            //    }
            //    Rebuild();
            //}
        }

        private void OnCustomOrderToggled(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                rootVisualElement.Q<IntegerField>("starting-value-int").style.display = DisplayStyle.Flex;
                rootVisualElement.Q<EnumField>("order-direction-enum").style.display = DisplayStyle.Flex;
            }
            else
            {
                rootVisualElement.Q<IntegerField>("starting-value-int").style.display = DisplayStyle.None;
                rootVisualElement.Q<EnumField>("order-direction-enum").style.display = DisplayStyle.None;
            }
        }

        private void AddEnumItem()
        {
            var view = rootVisualElement.Q<ScrollView>("enum-scroll");
            _enums.Add(new BackingContext());
            bool isOpen = view.style.display == DisplayStyle.Flex;
            if (isOpen)
                view.Add(CreateTextFieldElement(_enums.Count - 1, _enums[^1]));
        }

        private void RemoveEnumItem()
        {
            var view = rootVisualElement.Q<ScrollView>("enum-scroll");
            bool isOpen = view.style.display == DisplayStyle.Flex;
            if (isOpen)
            {
                for (int i = _enums.Count - 1; i >= 0; i--)
                {
                    if (_enums[i].toggled)
                    {
                        _enums.RemoveAt(i);
                    }
                }
                Clear(view);
                Rebuild(view);
            }
        }

        private void OnToggleListView(ChangeEvent<bool> x)
        {
            var view = rootVisualElement.Q<ScrollView>("enum-scroll");
            view.style.display = x.newValue ? DisplayStyle.Flex : DisplayStyle.None;

            if (x.newValue)
            {
                Rebuild(view);
            }
            else
            {
                Clear(view);
            }
        }

        public void Rebuild()
        {
            var view = rootVisualElement.Q<ScrollView>("enum-scroll");
            bool isOpen = view.style.display == DisplayStyle.Flex;
            if (isOpen)
            {
                Clear(view);
                Rebuild(view);
            }
        }

        private void Rebuild(ScrollView view)
        {
            var searchField = rootVisualElement.Q<ToolbarSearchField>("search-field");
            string filterWord = searchField.value;
            _filteredEnums.Clear();
            if (filterWord == null || filterWord == string.Empty || filterWord.Equals("None"))
            {
                _filteredEnums.AddRange(_enums);
            }
            else
            {
                foreach (var choice in _enums)
                {
                    if (FuzzySearch.FuzzyMatch(filterWord, choice.value))
                    {
                        _filteredEnums.Add(choice);
                    }
                }
            }

            int index = 0;
            foreach (var e in _filteredEnums)
            {
                var element = CreateTextFieldElement(index, e);
                view.Add(element);
                index++;
            }
        }

        public void Clear()
        {
            var view = rootVisualElement.Q<ScrollView>("enum-scroll");
            Clear(view);
        }

        private void Clear(ScrollView view)
        {
            for (int i = view.childCount - 1; i >= 0; i--)
            {
                view[i].RemoveFromHierarchy();
            }
        }

        private void OnSearchChanged(ChangeEvent<string> evt)
        {
            Rebuild();
        }

        private VisualElement CreateTextFieldElement(int index, BackingContext context)
        {
            var e = new VisualElement();
            e.style.flexDirection = FlexDirection.Row;
            e.style.marginBottom = 1;
            e.style.marginTop = 1;
            e.style.backgroundColor = index % 2 == 0 ? new StyleColor(new Color(0.20f, 0.20f, 0.20f)) : new StyleColor(new Color(0.22f, 0.22f, 0.22f));
            var toggle = new Toggle
            {
                tabIndex = -1
            };
            toggle.SetValueWithoutNotify(context.toggled);
            toggle.RegisterValueChangedCallback(x => context.toggled = x.newValue);
            var tf = new TextField($"Element {index}");
            tf.SetValueWithoutNotify(context.value);
            tf.RegisterValueChangedCallback(x => context.value = x.newValue);
            tf.style.flexGrow = 1;
            e.Add(toggle);
            e.Add(tf);
            return e;
        }
    }
}
