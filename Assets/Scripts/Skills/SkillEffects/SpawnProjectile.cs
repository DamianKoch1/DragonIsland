using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MOBA
{
    /// <summary>
    /// Used to spawn (homing) projectiles towards units / position
    /// </summary>
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

        /// <summary>
        /// Increases projectile lifespan / size / base damage
        /// </summary>
        public override void LevelUp()
        {
            base.LevelUp();
            projectileProperties.lifespan += projectileProperties.lifespanPerRank;
            projectileProperties.size += projectileProperties.sizePerRank;
            projectileProperties.baseDamage += projectileProperties.damagePerRank;
        }

        /// <summary>
        /// Calls SpawnSkillshotRPC for other clients
        /// </summary>
        /// <param name="_targetPos"></param>
        /// <param name="viewID">view id of the spawned projectile</param>
        private void SpawnSkillshotNetworked(Vector3 _targetPos, int viewID)
        {
            if (!photonView)
            {
                photonView = PhotonView.Get(this);
            }
            photonView.RPC(nameof(SpawnSkillshotRPC), RpcTarget.Others, _targetPos, viewID);
        }

        /// <summary>
        /// Calls SpawnHomingRPC for other clients
        /// </summary>
        /// <param name="targetViewID">view id of target unit</param>
        /// <param name="viewID">view id of the spawned projectile</param>
        private void SpawnHomingNetworked(int targetViewID, int viewID)
        {
            if (!photonView)
            {
                photonView = PhotonView.Get(this);
            }
            photonView.RPC(nameof(SpawnHomingRPC), RpcTarget.Others, target.GetViewID(), viewID);
        }


        /// <summary>
        /// Spawns a skillshot projectile at targetPos (only visuals, has no scaling and doesn't know owner stats), adds a photonView with given viewID to it
        /// </summary>
        /// <param name="targetPos"></param>
        /// <param name="viewID">view id of the spawned projectile</param>
        [PunRPC]
        public void SpawnSkillshotRPC(Vector3 targetPos, int viewID)
        {
            var projectile = Instantiate(projectilePrefab).GetComponent<Projectile>();
            projectile.InitializeSkillshot(owner, targetPos, projectileSpawnpoint.position, projectileProperties, scaling, ownerTeamID, null);
            projectile.gameObject.AddComponent<PhotonView>().ViewID = viewID;
            projectile.waitForDestroyRPC = true;
        }

        /// <summary>
        /// Spawns a homing projectile towards owner of targetViewID (only visuals, has no scaling and doesn't know owner stats), adds a photonView with given viewID to it
        /// </summary>
        /// <param name="targetViewID">view id of target unit</param>
        /// <param name="viewID">view id of the spawned projectile</param>
        [PunRPC]
        public void SpawnHomingRPC(int targetViewID, int viewID)
        {
            var projectile = Instantiate(projectilePrefab).GetComponent<Projectile>();
            projectile.Initialize(owner, targetViewID.GetUnitByID(), projectileSpawnpoint.position, projectileProperties, scaling, ownerTeamID, null);
            projectile.gameObject.AddComponent<PhotonView>().ViewID = viewID;
            projectile.waitForDestroyRPC = true;
        }

        /// <summary>
        /// Spawns a skillshot projectile towards position with scaling / saved stats locally, visuals only for other clients, adds PhotonView and allocates unique viewID
        /// </summary>
        /// <param name="targetPos"></param>
        public override void Activate(Vector3 targetPos)
        {
            base.Activate(targetPos);
            var projectile = Instantiate(projectilePrefab).GetComponent<Projectile>();
            projectile.InitializeSkillshot(owner, targetPos, projectileSpawnpoint.position, projectileProperties, scaling, ownerTeamID, ownerStatsAtActivation);
            var view = projectile.gameObject.AddComponent<PhotonView>();
            PhotonNetwork.AllocateViewID(view);

            SpawnSkillshotNetworked(targetPos, view.ViewID);
        }

        /// <summary>
        /// Spawns a homing projectile towards target with scaling / saved stats locally, visuals only for other clients, adds PhotonView and allocates unique viewID
        /// </summary>
        /// <param name="target"></param>
        public override void Activate(Unit target)
        {
            base.Activate(target);
            var projectile = Instantiate(projectilePrefab).GetComponent<Projectile>();
            projectile.Initialize(owner, target, projectileSpawnpoint.position, projectileProperties, scaling, ownerTeamID, ownerStatsAtActivation);
            var view = projectile.gameObject.AddComponent<PhotonView>();
            PhotonNetwork.AllocateViewID(view);

            SpawnHomingNetworked(target.GetViewID(), view.ViewID);
        }

        /// <summary>
        /// Spawns homing projectiles towards each target with scaling / saved stats locally, visuals only for other clients, adds PhotonView and allocates unique viewID
        /// </summary>
        /// <param name="targets"></param>
        public override void Activate<T>(UnitList<T> targets)
        {
            foreach (var target in targets)
            {
                Activate(target);
            }
            target = null;
        }

        /// <summary>
        /// If activated, can spawn projectiles each tick towards a position depending on settings
        /// </summary>
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
