using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MOBA
{
    public class SpawnProjectile : SkillEffect
    {

        [SerializeField]
        private ProjectileProperties projectileProperties;

        [SerializeField]
        private Transform projectileSpawnpoint;

        [SerializeField]
        private Projectile projectilePrefab;


        public override void Activate(Vector3 targetPos)
        {
            projectilePrefab.SpawnSkillshot(owner, targetPos, projectileSpawnpoint.position, projectileProperties, scalings);
        }

        public override void Activate(Unit target)
        {
            projectilePrefab.Spawn(owner, target, projectileSpawnpoint.position, projectileProperties, scalings);
        }

        public override void Tick()
        {
        }

        protected override void OnDeactivated()
        {
        }

    }
}
