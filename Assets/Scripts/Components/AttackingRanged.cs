using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class AttackingRanged : Attacking
    {

        [SerializeField]
        protected Projectile projectilePrefab;

        [SerializeField]
        private ProjectileProperties projectileProperties;

        public Transform projectileSpawnpoint;

        public override void Attack(Unit target)
        {
            if (animator)
            {
                animator.SetTrigger("attack");
            }
            projectilePrefab.SpawnHoming(projectileSize, target, projectileSpawnpoint.position, owner, owner.Stats.AtkDmg, projectileSpeed, HitMode.targetOnly, DamageType.physical, false, true);
            AttackAnimFinished();
        }


    }
}