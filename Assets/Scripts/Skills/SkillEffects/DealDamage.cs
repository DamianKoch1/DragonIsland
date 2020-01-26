using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class DealDamage : SkillEffect
    {
        [SerializeField]
        private float baseDamage;

        [SerializeField]
        private DamageType dmgType;


        public override void Activate(Vector3 targetPos, UnitStats ownerStats)
        {
            base.Activate(targetPos, ownerStats);
            Debug.LogError("Can't use DealDamage effect on a mousePos targeted skill, use SpawnAreaOfEffect instead! (Source: " + owner.name + ")");
        }

        public override void Activate(Unit target, UnitStats ownerStats)
        {
            base.Activate(target, ownerStats);
            var damage = new Damage(baseDamage + scaling.GetScalingDamageBonusOnTarget(ownerStats, target), dmgType, owner, target);
            damage.Inflict();
        }

        public override void Activate<T>(UnitList<T> targets, UnitStats ownerStats)
        {
            foreach (var target in targets)
            {
                var damage = new Damage(baseDamage + scaling.GetScalingDamageBonusOnTarget(ownerStats, target), dmgType, owner, target);
                damage.Inflict();
            }
            target = null;
        }

        public override void Tick(UnitStats ownerStats)
        {
            Debug.LogError("A DealDamage effect shouldn't tick, consider applying a DamageOverTime buff for better visibility instead!");
        }


        protected override void OnDeactivated()
        {
        }
    }
}
