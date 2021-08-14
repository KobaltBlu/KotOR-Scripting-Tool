using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NWN_Script
{

    static class StringHelper
    {
        /// <summary>
        /// Receives string and returns the string with its letters reversed.
        /// </summary>
        public static string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }
    }

}
