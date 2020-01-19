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

    public abstract class Structure : Unit
    {
        [Space]

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
                    structure.OnBeforeDeath += CheckTargetableRequirements;
                }
            }
        }

        protected override void Die(Unit killer)
        {
            isDestroyed = true;
            base.Die(killer);
        }

        public override Color GetHPColor()
        {
            if (IsAlly(PlayerController.Player))
            {
                return PlayerController.Instance.defaultColors.allyStructureHP;
            }
            return PlayerController.Instance.defaultColors.enemyStructureHP;
        }

        private void CheckTargetableRequirements()
        {
            ValidateUnitList(isUntargetableUntilDestroyed);
            switch (targetableUntilMode)
            {
                case UntargetableUntilMode.allDestroyed:
                    foreach (var structure in isUntargetableUntilDestroyed)
                    {
                        if (!structure.isDestroyed) return;
                    }
                    Targetable = true;
                    damageable = true;
                    return;
                case UntargetableUntilMode.anyDestroyed:
                    foreach (var structure in isUntargetableUntilDestroyed)
                    {
                        if (!structure.isDestroyed) continue;
                        Targetable = true;
                        damageable = true;
                        return;
                    }
                    return;
                default:
                    Debug.LogWarning("encountered invalid TargetableUntilMode!");
                    return;
            }

        }

        protected override void SetupBars()
        {
            Instantiate(statBars).GetComponent<UnitStatBars>()?.Initialize(this, 1, 2, true);
        }
    }
}
