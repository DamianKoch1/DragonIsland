using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public abstract class CustomBuff : Buff
    {
        public void Initialize(BuffProperties _properties, List<string> customArgs)
        {
            base.Initialize(_properties);
            Initialize(customArgs);
        }

        protected abstract void Initialize(List<string> customArgs);
    }
}
