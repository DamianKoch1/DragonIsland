using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{

    public enum UntargetableUntilMode
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
        private UntargetableUntilMode targetableUntilMode;

        protected bool isDestroyed;

        protected override void Initialize()
        {
            base.Initialize();
            isDestroyed = false;
            ValidateUnitList(isUntargetableUntilDestroyed);
            if (isUntargetableUntilDestroyed.Count > 0)
            {
                Targetable = false;
                damageable = false;
                foreach (var structure in isUntargetableUntilDestroyed)
                {
                    structure.OnDeath += () => CheckTargetableRequirements(structure);
                }
            }
        }

        public override Color GetHPColor()
        {
            if (IsAlly(ChampHUD.Player))
            {
                return ChampHUD.Instance.defaultColors.allyStructureHP;
            }
            return ChampHUD.Instance.defaultColors.enemyStructureHP;
        }

        private void CheckTargetableRequirements(Structure destroyedStructure)
        {
            switch (targetableUntilMode)
            {
                case UntargetableUntilMode.allDestroyed:
                    foreach (var structure in isUntargetableUntilDestroyed)
                    {
                        if (!structure.isDestroyed)
                        {
                            return;
                        }
                        Targetable = true;
                        damageable = true;
                    }
                    break;
                case UntargetableUntilMode.anyDestroyed:
                    foreach (var structure in isUntargetableUntilDestroyed)
                    {
                        if (structure.isDestroyed)
                        {
                            Targetable = true;
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
