using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class AttackingMelee : Attacking
    {

        /// No need for RPC here as dmg.Inflict() will do that
        public override void OnAtkAnimNotify()
        {
            var target = currTargetViewID.GetUnitByID();
            if (!target) return;
            if (target.IsDead) return;
            var dmg = new Damage(attackScaling.GetScalingDamageBonusOnTarget(owner, target), DamageType.physical, owner, target);
            dmg.Inflict();
        }
    }
}
