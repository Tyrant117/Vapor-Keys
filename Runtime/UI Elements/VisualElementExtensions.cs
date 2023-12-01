using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace VaporKeys
{
    public static class VisualElementExtensions
    {
        /// <summary>
        /// Gets the psuedo state value via reflection
        /// </summary>
        /// <param name="element"></param>
        /// <returns>The int flag value of <see cref="PseudoStates"/></returns>
        public static int GetPsuedoState(this VisualElement element)
        {                        
            return (int)element.GetType().GetProperty("pseudoStates", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(element);
        }

        public static void AddPsuedoState(this VisualElement element, int state)
        {
            int result = element.GetPsuedoState() | state;
            var enumType = element.GetType().GetProperty("pseudoStates", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(element).GetType();
            if (enumType != null && enumType.IsEnum)
            {
                object enumValue = Enum.ToObject(enumType, result);
                element.GetType().GetProperty("pseudoStates", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(element, enumValue);
            }
            else
            {
                Debug.Log("pseudoStates is not enum");
            }
        }

        public static void RemovePsuedoState(this VisualElement element, int state)
        {
            int result = element.GetPsuedoState();
            result &= ~state;
            var enumType = element.GetType().GetProperty("pseudoStates", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(element).GetType();
            if (enumType != null && enumType.IsEnum)
            {
                object enumValue = Enum.ToObject(enumType, result);
                element.GetType().GetProperty("pseudoStates", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(element, enumValue);
            }
            else
            {
                Debug.Log("pseudoStates is not enum");
            }
        }

        public static bool HasPseudoFlag(this VisualElement element, int flag)
        {
            int result = element.GetPsuedoState();
            return (result & flag) == flag; 
        }

        public static void PseudoXOR(this VisualElement element, int flag)
        {
            int result = element.GetPsuedoState();
            result ^= flag;
            var enumType = element.GetType().GetProperty("pseudoStates", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(element).GetType();
            if (enumType != null && enumType.IsEnum)
            {
                object enumValue = Enum.ToObject(enumType, result);
                element.GetType().GetProperty("pseudoStates", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(element, enumValue);
            }
            else
            {
                Debug.Log("pseudoStates is not enum");
            }
        }
    }
}
