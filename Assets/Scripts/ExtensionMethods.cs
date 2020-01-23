using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public static class ExtensionMethods
    {
        public static float Truncate(this float value, int digits)
        {
            double mult = Math.Pow(10.0, digits);
            double result = Math.Truncate(mult * value) / mult;
            return (float)result;
        }
    }
}
