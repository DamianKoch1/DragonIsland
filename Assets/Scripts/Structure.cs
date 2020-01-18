using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{

    public enum TargetableUntilMode
    {
        invalid = -1,
        allDestroyed = 0,
        anyDestroyed = 1,
    }

    public class Structure : Unit
    {
        [SerializeField]
        protected List<Structure> isUntargetableUntilDestroyed;

        [SerializeField]
        private TargetableUntilMode targetableUntilMode;

        protected bool isDestroyed;

        protected override void Initialize()
        {
            base.Initialize();
            isDestroyed = false;
            if (isUntargetableUntilDestroyed.Count > 0)
            {
                targetable = false;
                damageable = false;
                foreach (var structure in isUntargetableUntilDestroyed)
                {
                    if (!structure) continue;
                    structure.OnDeath += () => CheckTargetableRequirements(structure);
                }
            }
        }



        private void CheckTargetableRequirements(Structure destroyedStructure)
        {
            switch (targetableUntilMode)
            {
                case TargetableUntilMode.allDestroyed:
                    foreach (var structure in isUntargetableUntilDestroyed)
                    {
                        if (!structure.isDestroyed)
                        {
                            return;
                        }
                        targetable = true;
                        damageable = true;
                    }
                    break;
                case TargetableUntilMode.anyDestroyed:
                    foreach (var structure in isUntargetableUntilDestroyed)
                    {
                        if (structure.isDestroyed)
                        {
                            targetable = true;
                            damageable = true;
                            return;
                        }
                    }
                    break;
                default:
                    Debug.LogWarning("encountered invalid TargetableUntilMode!");
                    break;
            }

        }
    }
}
