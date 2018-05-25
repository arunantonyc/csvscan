using System;
using System.Collections.Generic;

namespace csvscan
{
    public class StringComparer : IEqualityComparer<string>
    {
        /// <summary>
        /// Compare two strings for exact-match, starts-with., ends-with & contains
        /// </summary>
        /// <param name="x">Source string</param>
        /// <param name="y">Compare string</param>
        /// <returns>True if equals else false</returns>
        public bool Equals(string x, string y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;
            //Check whether any of the compared objects is null.
            if (x is null || y is null)
                return false;

            // check for exact mactch
            if (!x.StartsWith("*") && !x.EndsWith("*"))
            {
                return y == x;
            }

            var xFixedStr = x.Replace("*", "");
            // check for starts with
            if (!x.StartsWith("*") && x.EndsWith("*"))
            {
                return y.StartsWith(xFixedStr);
            }
            // check for ends with
            if (x.StartsWith("*") && !x.EndsWith("*"))
            {
                return y.EndsWith(xFixedStr);
            }
            // check for contains
            if (x.StartsWith("*") && x.EndsWith("*"))
            {
                return y.Contains(xFixedStr);
            }
            return false;
        }
        /// <summary>
        /// Override for IEqualityComparer interface
        /// </summary>
        /// <param name="x">Source string</param>
        /// <returns>Returns has code if not null else 0</returns>
        public int GetHashCode(string x)
        {
            //Check whether the object is null
            if (x is null) return 0;

            return x.GetHashCode();
        }
    }
}
