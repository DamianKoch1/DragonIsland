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
        protected Transform projectileSpawnpoint;

        public override void Attack(Unit target)
        {
            projectile.SpawnHoming(target, projectileSpawnpoint.position, owner, owner.AtkDmg, projectileSpeed, HitMode.targetOnly, DamageType.physical);
        }

       
    }
}