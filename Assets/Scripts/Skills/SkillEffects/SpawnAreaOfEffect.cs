using Photon.Pun;
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
            if (delay == 0)
            {
                SpawnAOE(targetPos);
                return;
            }
            OnDelayFinished = () => SpawnAOE(targetPos);
            StartCoroutine(WaitForDelay());
        }

        public override void Activate(Unit target)
        {
            base.Activate(target);
            if (delay == 0)
            {
                SpawnAOE(target, ownerStatsAtActivation);
                return;
            }
            OnDelayFinished = () => SpawnAOE(target, ownerStatsAtActivation);
            StartCoroutine(WaitForDelay());
        }

        [PunRPC]
        public void SpawnAOEatPosNetworked(Vector3 targetPos)
        {
            Instantiate(areaOfEffectPrefab.gameObject, targetPos, Quaternion.identity);
        }

        [PunRPC]
        public void SpawnAOEonUnitNetworked(int parentViewID, Vector3 targetPos)
        {
            Instantiate(areaOfEffectPrefab.gameObject, targetPos, Quaternion.identity, parentViewID.GetUnitByID().transform);
        }

        private void SpawnAOE(Vector3 targetPos)
        {
            currentAOEInstance = Instantiate(areaOfEffectPrefab.gameObject, targetPos, Quaternion.identity).GetComponent<AreaOfEffect>();
            if (attachToTarget)
            {
                currentAOEInstance.transform.SetParent(owner.transform, true);

                photonView?.RPC(nameof(SpawnAOEonUnitNetworked), RpcTarget.Others, owner.GetViewID(), targetPos);
            }
            else photonView?.RPC(nameof(SpawnAOEatPosNetworked), RpcTarget.Others, targetPos);
            currentAOEInstance.Initialize(owner, ownerStatsAtActivation, ownerTeamID, null, lifespan, size, tickInterval, hitMode, canHitStructures, scaling);
        }

        private void SpawnAOE(Unit target, UnitStats ownerStats)
        {
            currentAOEInstance = Instantiate(areaOfEffectPrefab.gameObject, target.GetGroundPos(), Quaternion.identity).GetComponent<AreaOfEffect>();
            if (attachToTarget)
            {
                currentAOEInstance.transform.SetParent(target.transform, true);
                photonView?.RPC(nameof(SpawnAOEonUnitNetworked), RpcTarget.Others, target.GetViewID(), target.GetGroundPos());
            }
            else photonView?.RPC(nameof(SpawnAOEatPosNetworked), RpcTarget.Others, target.GetGroundPos());
            currentAOEInstance.Initialize(owner, ownerStats, ownerTeamID, target, lifespan, size, tickInterval, hitMode, canHitStructures, scaling);
        }

        private IEnumerator WaitForDelay()
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            OnDelayFinished?.Invoke();
        }

        private Action OnDelayFinished;

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
            if (!currentAOEInstance) return;
            Destroy(currentAOEInstance.gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, size / 2);
        }

    }
}
