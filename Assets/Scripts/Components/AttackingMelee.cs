using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    /// <summary>
    /// Melee attacking, instantly deals damage on animation event
    /// </summary>
    public class AttackingMelee : Attacking
    {

        /// <summary>
        /// Inflicts physical scaling dependent damage on current target if it is alive
        /// </summary>
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
