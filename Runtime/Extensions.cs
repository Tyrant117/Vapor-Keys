using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VaporKeys
{
    public static class Extensions
    {
        public static int GetKeyHashCode(this string text)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in text)
                {
                    hash = hash * 31 + c;
                }
                return hash;
            }
        }
    }
}
