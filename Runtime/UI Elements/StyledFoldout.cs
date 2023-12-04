using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace VaporKeys
{
    public class StyledFoldout
    {
        public static implicit operator VisualElement(StyledFoldout foldout) => foldout.OuterBox;

        public Box OuterBox { get; private set; }
        public Foldout Foldout { get; private set; }
        public Label Label { get; private set; }
        public VisualElement Content { get; private set; }

        public StyledFoldout(string header) : base()
        {
            StyleBox();
            StyleFoldout(header);

            OuterBox.Add(Foldout);
        }

        protected virtual void StyleBox()
        {
            OuterBox = new Box() { name = "styled-foldout-box" };
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
            OuterBox.style.marginLeft = 0;
            OuterBox.style.marginRight = 0;
            OuterBox.style.backgroundColor = ContainerStyles.BackgroundColor;
        }

        protected virtual void StyleFoldout(string header)
        {
            Foldout = new Foldout()
            {
                text = header,
                name = "styled-foldout-foldout"
            };

            var togStyle = Foldout.Q<Toggle>().style;
            togStyle.marginTop = 0;
            togStyle.marginLeft = 0;
            togStyle.marginRight = 0;
            togStyle.marginBottom = 0;
            togStyle.backgroundColor = ContainerStyles.HeaderColor;

            var togContainerStyle = Foldout.Q<Toggle>().hierarchy[0].style;
            togContainerStyle.marginLeft = 3;
            togContainerStyle.marginTop = 3;
            togContainerStyle.marginBottom = 3;

            // Label
            Label = Foldout.Q<Toggle>().Q<Label>();

            // Content
            Content = Foldout.Q<VisualElement>("unity-content");
            Content.style.marginTop = 3;
            Content.style.marginRight = 4;
            Content.style.marginBottom = 3;
            Content.style.marginLeft = 3;


            Foldout.value = false;
        }

        public void Add(VisualElement child)
        {
            Foldout.Add(child);
        }

        public void Remove(VisualElement child)
        {
            Foldout.Remove(child);
        }
    }
}
