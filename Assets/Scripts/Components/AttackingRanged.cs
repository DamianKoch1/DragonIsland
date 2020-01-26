using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class AttackingRanged : Attacking
    {
        [Space]
        [SerializeField, Tooltip("Assign a prefab with Projectile component")]
        protected Projectile projectilePrefab;

        [SerializeField]
        private ProjectileProperties projectileProperties = new ProjectileProperties()
        {
            isHoming = true,
            canHitStructures = true,
            speed = 10,
            size = 1,
            hitMode = HitMode.targetOnly,
            dmgType = DamageType.physical,
        };

        [Space]
        [SerializeField]
        private Scalings attackScaling = new Scalings() { ad = 1 };

        [Space]
        public Transform projectileSpawnpoint;

        public override void Attack(Unit target)
        {
            if (animator)
            {
                animator.SetTrigger("attack");
            }
            projectilePrefab.Spawn(owner, target, projectileSpawnpoint.position, projectileProperties, attackScaling, owner.TeamID, new UnitStats(owner.Stats));
            AttackAnimFinished();
        }


    }
}