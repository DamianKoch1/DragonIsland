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
        private float baseDamagePerRank;

        [SerializeField]
        private DamageType dmgType;


        public override void LevelUp()
        {
            base.LevelUp();
            baseDamage += baseDamagePerRank;
        }

        public override void Activate(Vector3 targetPos)
        {
            base.Activate(targetPos);
            Debug.LogError("Can't use DealDamage effect on a mousePos targeted skill, use SpawnAreaOfEffect instead! (Source: " + owner.name + ")");
        }

        public override void Activate(Unit target)
        {
            base.Activate(target);
            var damage = new Damage(baseDamage + scaling.GetScalingDamageBonusOnTarget(ownerStatsAtActivation, target), dmgType, owner, target);
            damage.Inflict();
        }

        public override void Activate<T>(UnitList<T> targets)
        {
            foreach (var target in targets)
            {
                var damage = new Damage(baseDamage + scaling.GetScalingDamageBonusOnTarget(ownerStatsAtActivation, target), dmgType, owner, target);
                damage.Inflict();
            }
            target = null;
        }


        public override void Tick()
        {
            Debug.LogError("A DealDamage effect shouldn't tick, consider applying a DamageOverTime buff for better visibility instead!");
        }


        protected override void OnDeactivated()
        {
        }
    }
}
