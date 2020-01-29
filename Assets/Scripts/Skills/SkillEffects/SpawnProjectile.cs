using Photon.Pun;
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

        [SerializeField, Tooltip("Assign a prefab with Projectile component")]
        private Projectile projectilePrefab;

        [Header("Toggle settings")]
        [SerializeField, Tooltip("Leave at false for non toggle skills.")]
        private bool spawnEachToggleTick;

        [SerializeField, Tooltip("If skill is mousePos targeted toggle, use current mouse pos or cast in original cast direction? (excludes remembering mouse pos)")]
        private bool rememberCastDirection = false;

        [SerializeField, Tooltip("If skill is mousePos targeted toggle, use current mouse pos or cast at original cast position?")]
        private bool rememberCastMousePos = false;

        public override void Activate(Vector3 targetPos, UnitStats ownerStats)
        {
            base.Activate(targetPos, ownerStats);
            projectilePrefab.SpawnSkillshot(owner, targetPos, projectileSpawnpoint.position, projectileProperties, scaling, ownerTeamID, ownerStats);
        }

        public override void Activate(Unit target, UnitStats ownerStats)
        {
            base.Activate(target, ownerStats);
            projectilePrefab.Spawn(owner, target, projectileSpawnpoint.position, projectileProperties, scaling, ownerTeamID, ownerStats);
        }

        public override void Activate<T>(UnitList<T> targets, UnitStats ownerStats)
        {
            foreach (var target in targets)
            {
                Activate(target, ownerStats);
            }
            target = null;
        }

        public override void Tick(UnitStats ownerStats)
        {
            if (!spawnEachToggleTick) return;
            if (target)
            {
                Activate(target, ownerStats);
            }
            else if (rememberCastDirection)
            {
                Activate(owner.GetGroundPos() + 5 * castTargetDir, ownerStats);
            }
            else if (rememberCastMousePos)
            {
                Activate(castTargetPos, ownerStats);
            }
            else if (owner is Champ)
            {
                //PlayerController.Instance.UpdateMousePos();
                Activate(PlayerController.PlayerCursorInfos[PhotonView.Get(owner).ViewID].position, ownerStats);
            }
        }

        protected override void OnDeactivated()
        {
        }

        private void OnDrawGizmosSelected()
        {
            if (projectileProperties.lifespan < 0) return;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, projectileProperties.speed * projectileProperties.lifespan);
        }
    }
}
