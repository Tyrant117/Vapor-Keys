using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

namespace VaporKeys.UIElements
{
    public class SearchableTextField : TextField
    {
        public FieldInfo _visualInput;
        private readonly int defaultIndex;
        private TextElement _textElement;

        public VisualElement VisualInput => (VisualElement)_visualInput.GetValue(this);

        public List<string> Choices { get; set; } = new();
        public List<string> FilteredChoices { get; set; } = new();
        public ComboBoxMenu ComboBox { get; set; }

        public event Action<string> SearchChanged;
        public event Action EndEdit;

        public SearchableTextField(string label, List<string> choices, int defaultIndex) : base(label)
        {
            _visualInput = typeof(BaseField<string>).GetField("m_VisualInput", BindingFlags.NonPublic | BindingFlags.Instance);
            _textElement = this.Q<TextElement>();
            VisualInput.RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<InputEvent>(OnValueChanged);
            Choices = choices;
            this.defaultIndex = defaultIndex;
            SetValueWithoutNotify(Choices[defaultIndex]);
        }

        private void OnValueChanged(InputEvent evt)
        {
            if (ComboBox != null)
            {
                ComboBox.Hide();
            }
            CreateComboBox();
            _textElement.AddPsuedoState((int)ExternalPseudoStates.Focus);
            SearchChanged?.Invoke(evt.newData);
            evt.StopPropagation();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (ComboBox != null)
            {
                ComboBox.Hide();
            }
            CreateComboBox();
            Focus();
            evt.StopPropagation();
        }

        private void CreateComboBox()
        {
            ComboBox = new ComboBoxMenu(this);
            ComboBox.scrollView.style.maxHeight = 250;
            if (AddMenuItems(ComboBox))
            {
                ComboBox.DropDown(VisualInput.worldBound, this, anchored: true);
            }
            else
            {
                ComboBox = null;
            }
        }


        public virtual bool AddMenuItems(ComboBoxMenu menu)
        {
            FilteredChoices.Clear();
            if (value == null || value == string.Empty || value.Equals("None"))
            {
                FilteredChoices.AddRange(Choices);
            }
            else
            {
                foreach (var choice in Choices)
                {
                    if (FuzzySearch.FuzzyMatch(value, choice))
                    {
                        FilteredChoices.Add(choice);
                    }
                }
            }

            var selectedItem = value;
            foreach (var item in FilteredChoices)
            {
                bool isChecked = EqualityComparer<string>.Default.Equals(item, selectedItem) && FilteredChoices.Contains(selectedItem);
                menu.AddItem(GetListItemToDisplay(item), isChecked, delegate
                {
                    ChangeValueFromMenu(item);
                });
            }
            return FilteredChoices.Count > 0;
        }

        public virtual string GetListItemToDisplay(string value)
        {
            return (value != null && Choices.Contains(value)) ? value.ToString() : string.Empty;
        }

        private void ChangeValueFromMenu(string menuItem)
        {
            SetValueWithoutNotify(menuItem);
            SearchChanged?.Invoke(value);
            EndEdit?.Invoke();
        }
    }
}
