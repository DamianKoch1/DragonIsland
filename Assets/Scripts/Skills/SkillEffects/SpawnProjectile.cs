using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
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


        public override void LevelUp()
        {
            base.LevelUp();
            projectileProperties.lifespan += projectileProperties.lifespanPerRank;
            projectileProperties.size += projectileProperties.sizePerRank;
            projectileProperties.baseDamage += projectileProperties.damagePerRank;
        }


        private void SpawnSkillshotNetworked(Vector3 _targetPos, int viewID)
        {
            if (!photonView)
            {
                photonView = PhotonView.Get(this);
            }
            photonView.RPC(nameof(SpawnSkillshotRPC), RpcTarget.Others, _targetPos, viewID);
        }

        private void SpawnHomingNetworked(int targetViewID, int viewID)
        {
            if (!photonView)
            {
                photonView = PhotonView.Get(this);
            }
            photonView.RPC(nameof(SpawnHomingRPC), RpcTarget.Others, target.GetViewID(), viewID);
        }


        [PunRPC]
        public void SpawnSkillshotRPC(Vector3 targetPos, int viewID)
        {
            var projectile = projectilePrefab.SpawnSkillshot(owner, targetPos, projectileSpawnpoint.position, projectileProperties, scaling, ownerTeamID, null);
            projectile.gameObject.AddComponent<PhotonView>().ViewID = viewID;
            projectile.waitForDestroyRPC = true;
        }

        [PunRPC]
        public void SpawnHomingRPC(int targetViewID, int viewID)
        {
            var projectile = projectilePrefab.Spawn(owner, targetViewID.GetUnitByID(), projectileSpawnpoint.position, projectileProperties, scaling, ownerTeamID, null);
            projectile.gameObject.AddComponent<PhotonView>().ViewID = viewID;
            projectile.waitForDestroyRPC = true;
        }

        public override void Activate(Vector3 targetPos)
        {
            base.Activate(targetPos);
            var projectile = projectilePrefab.SpawnSkillshot(owner, targetPos, projectileSpawnpoint.position, projectileProperties, scaling, ownerTeamID, ownerStatsAtActivation);
            var view = projectile.gameObject.AddComponent<PhotonView>();
            PhotonNetwork.AllocateViewID(view);

            SpawnSkillshotNetworked(targetPos, view.ViewID);
        }

        public override void Activate(Unit target)
        {
            base.Activate(target);
            var projectile = projectilePrefab.Spawn(owner, target, projectileSpawnpoint.position, projectileProperties, scaling, ownerTeamID, ownerStatsAtActivation);
            var view = projectile.gameObject.AddComponent<PhotonView>();
            PhotonNetwork.AllocateViewID(view);

            SpawnHomingNetworked(target.GetViewID(), view.ViewID);
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
