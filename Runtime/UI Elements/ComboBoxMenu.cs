using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace VaporKeys.UIElements
{
    public class ComboBoxMenu
    {
        public class MenuItem
        {
            public string name;

            public VisualElement element;

            public Action action;

            public Action<object> actionUserData;
        }

        //
        // Summary:
        //     USS class name of elements of this type.
        public static readonly string ussClassName = "unity-base-dropdown";

        //
        // Summary:
        //     USS class name of labels in elements of this type.
        public static readonly string itemUssClassName = ussClassName + "__item";

        //
        // Summary:
        //     USS class name of labels in elements of this type.
        public static readonly string labelUssClassName = ussClassName + "__label";

        //
        // Summary:
        //     USS class name of inner containers in elements of this type.
        public static readonly string containerInnerUssClassName = ussClassName + "__container-inner";

        //
        // Summary:
        //     USS class name of outer containers in elements of this type.
        public static readonly string containerOuterUssClassName = ussClassName + "__container-outer";

        //
        // Summary:
        //     USS class name of separators in elements of this type.
        public static readonly string checkmarkUssClassName = ussClassName + "__checkmark";

        //
        // Summary:
        //     USS class name of separators in elements of this type.
        public static readonly string separatorUssClassName = ussClassName + "__separator";

        private readonly SearchableTextField field;
        private List<MenuItem> m_Items = new();

        private VisualElement m_MenuContainer;

        private VisualElement m_OuterContainer;

        private ScrollView m_ScrollView;

        private VisualElement m_PanelRootVisualContainer;

        private VisualElement m_TargetElement;

        private Rect m_DesiredRect;

        private Vector2 m_MousePosition;

        public List<MenuItem> items => m_Items;

        public VisualElement menuContainer => m_MenuContainer;

        public VisualElement outerContainer => m_OuterContainer;

        public ScrollView scrollView => m_ScrollView;

        public bool isSingleSelectionDropdown { get; set; }

        public bool closeOnParentResize { get; set; }

        //
        // Summary:
        //     Returns the content container for the GenericDropdownMenu. Allows users to create
        //     their own dropdown menu if they don't want to use the default implementation.
        public VisualElement contentContainer => m_ScrollView.contentContainer;

        //
        // Summary:
        //     Initializes and returns an instance of GenericDropdownMenu.
        public ComboBoxMenu(SearchableTextField field)
        {
            this.field = field;
            m_MenuContainer = new VisualElement();
            m_MenuContainer.AddToClassList(ussClassName);
            m_OuterContainer = new VisualElement();
            m_OuterContainer.AddToClassList(containerOuterUssClassName);
            m_MenuContainer.Add(m_OuterContainer);
            m_ScrollView = new ScrollView();
            m_ScrollView.AddToClassList(containerInnerUssClassName);
            m_ScrollView.pickingMode = PickingMode.Position;
            m_ScrollView.contentContainer.focusable = true;
            m_ScrollView.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
            m_OuterContainer.hierarchy.Add(m_ScrollView);
            m_MenuContainer.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            m_MenuContainer.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            isSingleSelectionDropdown = true;
            closeOnParentResize = true;            
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            if (evt.destinationPanel != null)
            {
                //contentContainer.AddManipulator(m_NavigationManipulator = new KeyboardNavigationManipulator(Apply));
                m_MenuContainer.RegisterCallback<PointerDownEvent>(OnPointerDown);
                m_MenuContainer.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                m_MenuContainer.RegisterCallback<PointerUpEvent>(OnPointerUp);
                evt.destinationPanel.visualTree.RegisterCallback<GeometryChangedEvent>(OnParentResized);
                m_ScrollView.RegisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
                m_ScrollView.RegisterCallback<FocusInEvent>(OnFocusIn);
                m_ScrollView.RegisterCallback<FocusOutEvent>(OnFocusOut);
            }
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            if (evt.originPanel != null)
            {
                //contentContainer.RemoveManipulator(m_NavigationManipulator);
                m_MenuContainer.UnregisterCallback<PointerDownEvent>(OnPointerDown);
                m_MenuContainer.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                m_MenuContainer.UnregisterCallback<PointerUpEvent>(OnPointerUp);
                evt.originPanel.visualTree.UnregisterCallback<GeometryChangedEvent>(OnParentResized);
                m_ScrollView.UnregisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
                m_ScrollView.UnregisterCallback<FocusInEvent>(OnFocusIn);
                m_ScrollView.UnregisterCallback<FocusOutEvent>(OnFocusOut);
            }
        }

        

        public void Hide(bool giveFocusBack = false)
        {
            m_MenuContainer.RemoveFromHierarchy();
            if (m_TargetElement != null)
            {
                //m_TargetElement.PseudoXOR((int)ExternalPseudoStates.Active);
                //if (giveFocusBack)
                //{
                //    m_TargetElement.Focus();
                //}
            }

            m_TargetElement = null;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            m_MousePosition = m_ScrollView.WorldToLocal(evt.position);
            UpdateSelection(evt.target as VisualElement);
            if (evt.pointerId != PointerId.mousePointerId)
            {
                //m_MenuContainer.panel.PreventCompatibilityMouseEvents(evt.pointerId);
            }
            if (!m_ScrollView.ContainsPoint(m_MousePosition))
            {
                Hide();
            }
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            m_MousePosition = m_ScrollView.WorldToLocal(evt.position);
            UpdateSelection(evt.target as VisualElement);
            if (evt.pointerId != PointerId.mousePointerId)
            {
                //m_MenuContainer.panel.PreventCompatibilityMouseEvents(evt.pointerId);
            }

            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            int selectedIndex = GetSelectedIndex();
            if (selectedIndex != -1)
            {
                MenuItem menuItem = m_Items[selectedIndex];
                menuItem.action?.Invoke();
                menuItem.actionUserData?.Invoke(menuItem.element.userData);
                if (isSingleSelectionDropdown)
                {
                    Hide(/*giveFocusBack: true*/);
                }
            }

            if (evt.pointerId != PointerId.mousePointerId)
            {
                //m_MenuContainer.panel.PreventCompatibilityMouseEvents(evt.pointerId);
            }

            evt.StopPropagation();
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            //Debug.Log("Returning Focus");
            //field.Focus();
        }

        private void OnFocusOut(FocusOutEvent evt)
        {
            if (!m_ScrollView.ContainsPoint(m_MousePosition))
            {
                Hide();
            }
            else
            {
                //m_MenuContainer.schedule.Execute(contentContainer.Focus);
            }
        }

        private void OnParentResized(GeometryChangedEvent evt)
        {
            if (closeOnParentResize)
            {
                Hide(/*giveFocusBack: true*/);
            }
        }

        private void UpdateSelection(VisualElement target)
        {
            if (!m_ScrollView.ContainsPoint(m_MousePosition))
            {
                int selectedIndex = GetSelectedIndex();
                if (selectedIndex >= 0)
                {
                    m_Items[selectedIndex].element.RemovePsuedoState((int)ExternalPseudoStates.Hover);
                }
            }
            else if (target != null && ((int)target.GetPsuedoState() & (int)ExternalPseudoStates.Hover) != (int)ExternalPseudoStates.Hover)
            {
                int selectedIndex2 = GetSelectedIndex();
                if (selectedIndex2 >= 0)
                {
                    m_Items[selectedIndex2].element.RemovePsuedoState((int)ExternalPseudoStates.Hover);
                }

                target.AddPsuedoState((int)ExternalPseudoStates.Hover);
            }
        }

        private int GetSelectedIndex()
        {
            for (int i = 0; i < m_Items.Count; i++)
            {
                if (((int)m_Items[i].element.GetPsuedoState() & (int)ExternalPseudoStates.Hover) == (int)ExternalPseudoStates.Hover)
                {
                    return i;
                }
            }

            return -1;
        }

        //
        // Summary:
        //     Adds an item to this menu using a default VisualElement.
        //
        // Parameters:
        //   itemName:
        //     The text to display to the user.
        //
        //   isChecked:
        //     Indicates whether a checkmark next to the item is displayed.
        //
        //   action:
        //     The callback to invoke when the item is selected by the user.
        public void AddItem(string itemName, bool isChecked, Action action)
        {
            MenuItem menuItem = AddItem(itemName, isChecked, isEnabled: true);
            if (menuItem != null)
            {
                menuItem.action = action;
            }
        }

        public void AddItem(string itemName, bool isChecked, Action<object> action, object data)
        {
            MenuItem menuItem = AddItem(itemName, isChecked, isEnabled: true, data);
            if (menuItem != null)
            {
                menuItem.actionUserData = action;
            }
        }

        //
        // Summary:
        //     Adds a disabled item to this menu using a default VisualElement.
        //
        // Parameters:
        //   itemName:
        //     The text to display to the user.
        //
        //   isChecked:
        //     Indicates whether a checkmark next to the item is displayed.
        public void AddDisabledItem(string itemName, bool isChecked)
        {
            AddItem(itemName, isChecked, isEnabled: false);
        }

        //
        // Summary:
        //     Adds a visual separator after the previously added items in this menu.
        //
        // Parameters:
        //   path:
        //     Not used.
        public void AddSeparator(string path)
        {
            VisualElement visualElement = new();
            visualElement.AddToClassList(separatorUssClassName);
            visualElement.pickingMode = PickingMode.Ignore;
            m_ScrollView.Add(visualElement);
        }

        private MenuItem AddItem(string itemName, bool isChecked, bool isEnabled, object data = null)
        {
            if (string.IsNullOrEmpty(itemName) || itemName.EndsWith("/"))
            {
                AddSeparator(itemName);
                return null;
            }

            for (int i = 0; i < m_Items.Count; i++)
            {
                if (itemName == m_Items[i].name)
                {
                    return null;
                }
            }

            VisualElement visualElement = new();
            visualElement.AddToClassList(itemUssClassName);
            visualElement.SetEnabled(isEnabled);
            visualElement.userData = data;
            VisualElement visualElement2 = new();
            visualElement2.AddToClassList(checkmarkUssClassName);
            visualElement2.pickingMode = PickingMode.Ignore;
            visualElement.Add(visualElement2);
            if (isChecked)
            {
                visualElement.AddPsuedoState((int)ExternalPseudoStates.Checked);
            }

            Label label = new(itemName);
            label.AddToClassList(labelUssClassName);
            label.pickingMode = PickingMode.Ignore;
            visualElement.Add(label);
            m_ScrollView.Add(visualElement);
            MenuItem menuItem = new()
            {
                name = itemName,
                element = visualElement
            };
            m_Items.Add(menuItem);
            return menuItem;
        }

        internal void UpdateItem(string itemName, bool isChecked)
        {
            MenuItem menuItem = m_Items.Find((MenuItem x) => x.name == itemName);
            if (menuItem != null)
            {
                if (isChecked)
                {
                    menuItem.element.AddPsuedoState((int)ExternalPseudoStates.Checked);
                }
                else
                {
                    menuItem.element.RemovePsuedoState((int)ExternalPseudoStates.Checked);
                }
            }
        }

        //
        // Summary:
        //     Displays the menu at the specified position.
        //
        // Parameters:
        //   position:
        //     The position in the coordinate space of the panel.
        //
        //   targetElement:
        //     The element used to determine in which root to parent the menu.
        //
        //   anchored:
        //     Whether the menu should use the width of the position argument instead of its
        //     normal width.
        public void DropDown(Rect position, VisualElement targetElement = null, bool anchored = false)
        {
            if (targetElement == null)
            {
                Debug.LogError("VisualElement Generic Menu needs a target to find a root to attach to.");
                return;
            }

            m_TargetElement = targetElement;
            m_TargetElement.RegisterCallback<DetachFromPanelEvent>(OnTargetElementDetachFromPanel);
            m_PanelRootVisualContainer = (VisualElement)m_TargetElement.GetType().GetMethod("GetRootVisualContainer", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(m_TargetElement, null);
            if (m_PanelRootVisualContainer == null)
            {
                Debug.LogError("Could not find rootVisualContainer...");
                return;
            }

            m_PanelRootVisualContainer.Add(m_MenuContainer);
            //m_MenuContainer.focusable = false;
            m_MenuContainer.style.left = m_PanelRootVisualContainer.layout.x;
            m_MenuContainer.style.top = m_PanelRootVisualContainer.layout.y;
            m_MenuContainer.style.width = m_PanelRootVisualContainer.layout.width;
            m_MenuContainer.style.height = m_PanelRootVisualContainer.layout.height;
            m_MenuContainer.style.fontSize = m_TargetElement.resolvedStyle.fontSize;
            m_MenuContainer.style.unityFont = m_TargetElement.resolvedStyle.unityFont;
            m_MenuContainer.style.unityFontDefinition = m_TargetElement.resolvedStyle.unityFontDefinition;
            Rect rect = m_PanelRootVisualContainer.WorldToLocal(position);
            m_OuterContainer.style.left = rect.x - m_PanelRootVisualContainer.layout.x;
            m_OuterContainer.style.top = rect.y + position.height - m_PanelRootVisualContainer.layout.y;
            m_DesiredRect = (anchored ? position : Rect.zero);
            //m_MenuContainer.schedule.Execute(contentContainer.Focus);
            m_ScrollView.AddPsuedoState((int)ExternalPseudoStates.Focus);
            EnsureVisibilityInParent();
            if (targetElement != null)
            {
                targetElement.AddPsuedoState((int)ExternalPseudoStates.Active);
            }
        }

        private void OnTargetElementDetachFromPanel(DetachFromPanelEvent evt)
        {
            Hide();
        }

        private void OnContainerGeometryChanged(GeometryChangedEvent evt)
        {
            EnsureVisibilityInParent();
        }

        private void EnsureVisibilityInParent()
        {
            if (m_PanelRootVisualContainer != null && !float.IsNaN(m_OuterContainer.layout.width) && !float.IsNaN(m_OuterContainer.layout.height))
            {
                if (m_DesiredRect == Rect.zero)
                {
                    float num = Mathf.Min(m_OuterContainer.layout.x, m_PanelRootVisualContainer.layout.width - m_OuterContainer.layout.width);
                    float num2 = Mathf.Min(m_OuterContainer.layout.y, Mathf.Max(0f, m_PanelRootVisualContainer.layout.height - m_OuterContainer.layout.height));
                    m_OuterContainer.style.left = num;
                    m_OuterContainer.style.top = num2;
                }

                m_OuterContainer.style.height = Mathf.Min(m_MenuContainer.layout.height - m_MenuContainer.layout.y - m_OuterContainer.layout.y, m_ScrollView.layout.height + m_OuterContainer.resolvedStyle.borderBottomWidth + m_OuterContainer.resolvedStyle.borderTopWidth);
                if (m_DesiredRect != Rect.zero)
                {
                    m_OuterContainer.style.width = m_DesiredRect.width;
                }
            }
        }
    }
}
