using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VaporKeys
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class KeyDropdownAttribute : PropertyAttribute
    {
        public string ValuesName { get; private set; }

        public KeyDropdownAttribute(string valuesName)
        {
            ValuesName = valuesName;
        }
    }
}
