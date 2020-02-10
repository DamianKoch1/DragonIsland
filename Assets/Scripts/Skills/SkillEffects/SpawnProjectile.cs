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


        [PunRPC]
        public void SpawnSkillshotNetworked(Vector3 targetPos)
        {
            projectilePrefab.SpawnSkillshot(owner, targetPos, projectileSpawnpoint.position, projectileProperties, scaling, ownerTeamID, ownerStatsAtActivation);

        }

        [PunRPC]
        public void SpawnHomingNetworked(int targetViewID)
        {
            projectilePrefab.Spawn(owner, targetViewID.GetUnitByID(), projectileSpawnpoint.position, projectileProperties, scaling, ownerTeamID, ownerStatsAtActivation);
        }

        public override void Activate(Vector3 targetPos)
        {
            base.Activate(targetPos);
            projectilePrefab.SpawnSkillshot(owner, targetPos, projectileSpawnpoint.position, projectileProperties, scaling, ownerTeamID, ownerStatsAtActivation);

            photonView?.RPC(nameof(SpawnSkillshotNetworked), RpcTarget.Others, targetPos);
        }

        public override void Activate(Unit target)
        {
            base.Activate(target);
            projectilePrefab.Spawn(owner, target, projectileSpawnpoint.position, projectileProperties, scaling, ownerTeamID, ownerStatsAtActivation);

            photonView?.RPC(nameof(SpawnHomingNetworked), RpcTarget.Others, target.GetViewID());
        }

        public override void Activate<T>(UnitList<T> targets)
        {
            foreach (var target in targets)
            {
                Activate(target);
            }
            target = null;
        }

        public override void Tick()
        {
            if (!spawnEachToggleTick) return;
            if (target)
            {
                Activate(target);
            }
            else if (rememberCastDirection)
            {
                Activate(owner.GetGroundPos() + 5 * castTargetDir);
            }
            else if (rememberCastMousePos)
            {
                Activate(castTargetPos);
            }
            else if (owner is Champ)
            {
                PlayerController.Instance.GetMouseWorldPos(out var mousePos);
                Activate(mousePos);
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
