using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    /// <summary>
    /// Ranged attacking, spawns homing projectile on animation event
    /// </summary>
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
        public Transform projectileSpawnpoint;


        /// <summary>
        /// Spawns a homing projectile at projectileSpawnpoint following currentTarget
        /// </summary>
        public override void OnAtkAnimNotify()
        {
            base.OnAtkAnimNotify();
            var target = currTargetViewID.GetUnitByID();
            if (!target) return;
            if (target.IsDead) return;
            var projectile = Instantiate(projectilePrefab).GetComponent<Projectile>();
            projectile.Initialize(owner, target, projectileSpawnpoint.position, projectileProperties, attackScaling, owner.TeamID, new UnitStats(owner.Stats));
        }
    }
}