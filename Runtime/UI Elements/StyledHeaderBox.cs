using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace VaporKeys
{
    public class StyledHeaderBox
    {
        public static implicit operator VisualElement(StyledHeaderBox headerBox) => headerBox.OuterBox;

        public Box OuterBox { get; private set; }
        public Label Label { get; private set; }

        public StyledHeaderBox(string header)
        {
            StyleBox();
            StyleHeader(header);
            OuterBox.Add(Label);
        }

        protected virtual void StyleBox()
        {
            OuterBox = new Box() { name = "styled-header-box" };
            OuterBox.style.borderBottomColor = ContainerStyles.BorderColor;
            OuterBox.style.borderTopColor = ContainerStyles.BorderColor;
            OuterBox.style.borderRightColor = ContainerStyles.BorderColor;
            OuterBox.style.borderLeftColor = ContainerStyles.BorderColor;
            OuterBox.style.borderBottomLeftRadius = 3;
            OuterBox.style.borderBottomRightRadius = 3;
            OuterBox.style.borderTopLeftRadius = 3;
            OuterBox.style.borderTopRightRadius = 3;
            OuterBox.style.marginTop = 3;
            OuterBox.style.marginBottom = 3;
            OuterBox.style.marginLeft = 3;
            OuterBox.style.marginRight = 3;
            OuterBox.style.paddingBottom = 3;
            OuterBox.style.backgroundColor = ContainerStyles.BackgroundColor;
        }

        protected virtual void StyleHeader(string header)
        {
            Label = new Label(header) { name = "styled-header-box-label"};
            Label.style.paddingLeft = 6;
            Label.style.paddingTop = 3;
            Label.style.paddingBottom = 3;
            Label.style.borderBottomWidth = 1f;
            Label.style.borderBottomColor = ContainerStyles.BorderColor;
            Label.style.marginBottom = 6;
            Label.style.backgroundColor = ContainerStyles.HeaderColor;
        }

        public void Add(VisualElement child)
        {
            OuterBox.Add(child);
        }

        public void Remove(VisualElement child)
        {
            OuterBox.Remove(child);
        }
    }
}
