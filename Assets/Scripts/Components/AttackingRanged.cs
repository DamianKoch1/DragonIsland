using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class AttackingRanged : Attacking
    {

        [SerializeField]
        protected Projectile projectile;

        [SerializeField]
        protected float projectileSpeed;

        [SerializeField]
        protected float projectileSize = 1;

        public Transform projectileSpawnpoint;

        public override void Attack(Unit target)
        {
            if (animator)
            {
                animator.SetTrigger("attack");
            }
            projectile.SpawnHoming(projectileSize, target, projectileSpawnpoint.position, owner, owner.AtkDmg, projectileSpeed, HitMode.targetOnly, DamageType.physical, false, true);
            AttackAnimFinished();
        }


    }
}