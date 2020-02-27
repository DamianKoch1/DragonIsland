using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    //TODO WIP
    public abstract class CustomBuff : Buff
    {
        public void Initialize(BuffProperties _properties, UnitStats ownerStats, List<string> customArgs)
        {
            base.Initialize(_properties, ownerStats);
            if (customArgs.Count == 0) return;
            BuffName += " (" + customArgs[0] + ")";
            customArgs.RemoveAt(0);
            if (customArgs.Count == 0) return;
            Initialize(customArgs);
        }

        protected abstract void Initialize(List<string> customArgs);
    }
}
