using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MOBA
{
    public class SpawnProjectile : SkillEffect
    {
        [SerializeField]
        private Projectile projectilePrefab;

        [SerializeField]
        private ProjectileProperties projectileProperties;

        [SerializeField]
        private Transform projectileSpawnpoint;


        public override void Activate(Vector3 targetPos)
        {
            if (projectileProperties.isHoming) return;
            projectilePrefab.SpawnSkillshot(owner, targetPos, projectileSpawnpoint.position, projectileProperties, ProjectileScalingDamageBonus);
        }

        public override void Activate(Unit target)
        {
            projectilePrefab.Spawn(owner, target, projectileSpawnpoint.position, projectileProperties, GetProjectileDamageBonusOnTarget(target));
        }

        public override void Tick()
        {
        }

        protected override void OnDeactivated()
        {
        }

        private float ProjectileScalingDamageBonus => scalings * owner;

        private float GetProjectileDamageBonusOnTarget(Unit target)
        {
            return ProjectileScalingDamageBonus + scalings.GetScalingDamageBonusOnTarget(owner, target);
        }
    }
}
