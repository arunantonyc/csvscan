using System;
using System.Collections.Generic;

namespace csvscan
{
    public class StringComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            //Check whether the compared objects reference the same data.
            if (Object.ReferenceEquals(x, y)) return true;
            //Check whether any of the compared objects is null.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            // checks for absolute search string
            if (!x.StartsWith("*") && !x.EndsWith("*"))
            {
                return y == x;
            }

            var xFixedStr = x.Replace("*", "");
            // checking for starts with
            if (!x.StartsWith("*") && x.EndsWith("*"))
            {
                return y.StartsWith(xFixedStr);
            }
            // checking for ends with
            if (x.StartsWith("*") && !x.EndsWith("*"))
            {
                return y.EndsWith(xFixedStr);
            }
            // checking for contains
            if (x.StartsWith("*") && x.EndsWith("*"))
            {
                return y.Contains(xFixedStr);
            }
            return false;
        }

        public int GetHashCode(string x)
        {
            //Check whether the object is null
            if (Object.ReferenceEquals(x, null)) return 0;

            return x.GetHashCode();
        }
    }
}
