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

        public override void Activate(Vector3 targetPos, UnitStats ownerStats)
        {
            base.Activate(targetPos, ownerStats);
            if (delay == 0)
            {
                SpawnAOE(targetPos, ownerStats);
                return;
            }
            OnDelayFinished = () => SpawnAOE(targetPos, ownerStats);
            StartCoroutine(WaitForDelay());
        }

        public override void Activate(Unit target, UnitStats ownerStats)
        {
            base.Activate(target, ownerStats);
            if (delay == 0)
            {
                SpawnAOE(target, ownerStats);
                return;
            }
            OnDelayFinished = () => SpawnAOE(target, ownerStats);
            StartCoroutine(WaitForDelay());
        }

        private void SpawnAOE(Vector3 targetPos, UnitStats ownerStats)
        {
            currentAOEInstance = Instantiate(areaOfEffectPrefab.gameObject, targetPos, Quaternion.identity).GetComponent<AreaOfEffect>();
            if (attachToTarget)
            {
                currentAOEInstance.transform.SetParent(owner.transform, true);
            }
            currentAOEInstance.Initialize(owner, ownerStats, ownerTeamID, null, lifespan, size, tickInterval, hitMode, canHitStructures, scaling);
        }

        private void SpawnAOE(Unit target, UnitStats ownerStats)
        {
            currentAOEInstance = Instantiate(areaOfEffectPrefab.gameObject, target.GetGroundPos(), Quaternion.identity).GetComponent<AreaOfEffect>();
            if (attachToTarget)
            {
                currentAOEInstance.transform.SetParent(target.transform, true);
            }
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
            else if (PlayerController.Instance.GetMouseWorldPos(out var mouseWorldPos))
            {
                Activate(mouseWorldPos, ownerStats);
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
