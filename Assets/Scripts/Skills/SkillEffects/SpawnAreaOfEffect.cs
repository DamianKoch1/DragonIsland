using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class SpawnAreaOfEffect : SkillEffect
    {
        [Space]
        [SerializeField, Tooltip("Assign a prefab with AreaOfEffect component")]
        private AreaOfEffect areaOfEffectPrefab;

        [SerializeField, Tooltip("Spawn delay in seconds after cast time finished")]
        private float delay;

        [SerializeField, Tooltip("Will stay on target unit when true. If target is mousePos, stays on owner")]
        private bool attachToTarget;

        private AreaOfEffect currentAOEInstance;

        [SerializeField, Range(-1, 60), Tooltip("Leave at -1 for infinite.")]
        private float lifespan = -1;

        [SerializeField, Range(0.1f, 100)]
        private float size = 5;

        [SerializeField, Range(0.1f, 2)]
        private float tickInterval = 0.5f;

        [SerializeField]
        private HitMode hitMode = HitMode.enemyUnits;

        [SerializeField]
        private bool canHitStructures;

        [Header("Toggle settings")]
        [SerializeField, Tooltip("Leave at false for non toggle skills.")]
        private bool spawnEachToggleTick;

        [SerializeField, Tooltip("If skill is mousePos targeted toggle, use current mouse pos or cast in original cast direction? (excludes remembering mouse pos)")]
        private bool rememberCastDirection;

        [SerializeField, Tooltip("If skill is mousePos targeted toggle, use current mouse pos or cast at original cast position?")]
        private bool rememberCastMousePos;



        public override void Activate(Vector3 targetPos)
        {
            base.Activate(targetPos);
            SpawnAOE(targetPos);
        }

        public override void Activate(Unit target)
        {
            base.Activate(target);
            SpawnAOE(target, ownerStatsAtActivation);
        }


        private void SpawnAOEatPosNetworked(Vector3 targetPos, int viewID)
        {
            if (!photonView)
            {
                photonView = PhotonView.Get(this);
            }
            photonView.RPC(nameof(SpawnAOEatPosRPC), RpcTarget.Others, targetPos, viewID);
            
        }

        private void SpawnAOEonUnitNetworked(int _parentViewID, Vector3 targetPos, int viewID)
        {
            if (!photonView)
            {
                photonView = PhotonView.Get(this);
            }
            photonView.RPC(nameof(SpawnAOEonUnitRPC), RpcTarget.Others, _parentViewID, targetPos, viewID);
            
        }

        [PunRPC]
        public void SpawnAOEatPosRPC(Vector3 targetPos, int viewID)
        {
            currentAOEInstance = Instantiate(areaOfEffectPrefab.gameObject, targetPos, Quaternion.identity).GetComponent<AreaOfEffect>();
            currentAOEInstance.Initialize(owner, null, ownerTeamID, null, lifespan, size * 2, tickInterval, hitMode, canHitStructures, scaling, delay);
            currentAOEInstance.gameObject.AddComponent<PhotonView>().ViewID = viewID;
        }

        [PunRPC]
        public void SpawnAOEonUnitRPC(int parentViewID, Vector3 targetPos, int viewID)
        {
            currentAOEInstance = Instantiate(areaOfEffectPrefab.gameObject, targetPos, parentViewID.GetUnitByID().transform.rotation, parentViewID.GetUnitByID().transform).GetComponent<AreaOfEffect>();
            currentAOEInstance.Initialize(owner, null, ownerTeamID, target, lifespan, size * 2, tickInterval, hitMode, canHitStructures, scaling, delay);
            currentAOEInstance.gameObject.AddComponent<PhotonView>().ViewID = viewID;

        }

        private void SpawnAOE(Vector3 targetPos)
        {
            currentAOEInstance = Instantiate(areaOfEffectPrefab.gameObject, targetPos, Quaternion.identity).GetComponent<AreaOfEffect>();
            var view = currentAOEInstance.gameObject.AddComponent<PhotonView>();
            PhotonNetwork.AllocateViewID(view);
            
            if (attachToTarget)
            {
                currentAOEInstance.transform.SetParent(owner.transform, true);
                SpawnAOEonUnitNetworked(owner.GetViewID(), targetPos, view.ViewID);
            }
            else SpawnAOEatPosNetworked(targetPos, view.ViewID);

            currentAOEInstance.Initialize(owner, ownerStatsAtActivation, ownerTeamID, null, lifespan, size * 2, tickInterval, hitMode, canHitStructures, scaling, delay);
        }

        private void SpawnAOE(Unit target, UnitStats ownerStats)
        {
            currentAOEInstance = Instantiate(areaOfEffectPrefab.gameObject, target.GetGroundPos(), target.transform.rotation).GetComponent<AreaOfEffect>();
            var view = currentAOEInstance.gameObject.AddComponent<PhotonView>();
            PhotonNetwork.AllocateViewID(view);

            if (attachToTarget)
            {
                currentAOEInstance.transform.SetParent(target.transform, true);
                SpawnAOEonUnitNetworked(target.GetViewID(), target.GetGroundPos(), view.ViewID);
            }
            else SpawnAOEatPosNetworked(target.GetGroundPos(), view.ViewID);

            currentAOEInstance.Initialize(owner, ownerStats, ownerTeamID, target, lifespan, size * 2, tickInterval, hitMode, canHitStructures, scaling, delay);
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
            photonView.RPC(nameof(DestroyLastSpawned), RpcTarget.All);
        }

        [PunRPC]
        public void DestroyLastSpawned()
        {
            if (!currentAOEInstance) return;
            Destroy(currentAOEInstance.gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, size);
        }

    }
}
