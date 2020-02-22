using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class Dash : SkillEffect
    {
        [Space]
        [SerializeField, Range(0.1f, 10)]
        private float duration = 0;

        [Space]
        [SerializeField, Range(0, 149.9f)]
        private float minRange = 0;

        [SerializeField, Range(0.1f, 150)]
        private float maxRange = 20;


        [SerializeField, Tooltip("Time and value should range from 0 to 1")]
        private AnimationCurve distancePerTime = AnimationCurve.Linear(0, 0, 1, 1);

        [Space]
        [SerializeField, Range(0, 5)]
        private float maxHeight;


        [SerializeField, Tooltip("Time and value should range from 0 to 1")]
        private AnimationCurve heightPerTime = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

        [Space]
        [SerializeField, Range(-1, 4)]
        private float cameraZoom = -1;
        private float prevZoom;
        private bool wasCamUnlocked;

        private Unit prevAttackTarget;

        private bool isDashing;

        private Coroutine dashCoroutine;

        private IEnumerator DashCoroutine(Vector3 targetPos)
        {
            StartDashLock();

            var startPos = owner.transform.position;
            if (minRange > maxRange)
            {
                Debug.LogError("Dash minRange needs to be < lesser than maxRange!");
                yield break;
            }

            var distance = Vector3.Distance(startPos, targetPos);
            if (distance < minRange)
            {
                targetPos = startPos + (targetPos - startPos).normalized * minRange;
            }
            else if (distance > maxRange)
            {
                targetPos = startPos + (targetPos - startPos).normalized * maxRange;
            }
            ValidateTargetPos(targetPos, out targetPos);

            float timePassed = 0;
            while (timePassed <= duration)
            {
                float normalizedTime = timePassed / duration;
                var newPos = Vector3.Lerp(startPos, targetPos, distancePerTime.Evaluate(normalizedTime));
                if (maxHeight > 0)
                {
                    newPos += Vector3.up * heightPerTime.Evaluate(normalizedTime) * maxHeight;
                }

                owner.transform.position = newPos;

                timePassed += Time.deltaTime;
                yield return null;
            }
            owner.transform.position = targetPos;

            StopDashLock();
        }

        private void StartDashLock()
        {
            owner.CanMove = false;
            prevAttackTarget = null;
            if (owner.IsAttacking())
            {
                prevAttackTarget = owner.CurrentAttackTarget;
                owner.StopAttacking();
            }
            owner.canAttack = false;
            owner.canCast = false;
            isDashing = true;

            if (cameraZoom > 0)
            {
                var cam = ChampCamera.Instance;
                wasCamUnlocked = cam.Unlocked;
                prevZoom = cam.CurrentZoom;
                cam.Lock();
                cam.SetZoom(cameraZoom);
                cam.DisableControls();
            }
        }

        private void StopDashLock()
        {
            if (!isDashing) return;

            isDashing = false;
            owner.canAttack = true;
            owner.canCast = true;
            owner.CanMove = true;
            if (cameraZoom > 0)
            {
                var cam = ChampCamera.Instance;
                cam.EnableControls();
                if (wasCamUnlocked)
                {
                    cam.Unlock();
                }
                cam.SetZoom(prevZoom);
            }
            if (owner.IsDead) return;
            if (prevAttackTarget)
            {
                owner.StartAttacking(prevAttackTarget);
            }
            else if (owner.GetDestination() != Vector3.zero)
            {
                owner.MoveTo(owner.GetDestination());
            }
        }

        public override void Activate(Unit _target)
        {
            base.Activate(_target);
            var targetPos = _target.GetGroundPos();
            targetPos -= (targetPos - owner.GetGroundPos()).normalized;
            targetPos.y = owner.transform.position.y;
            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
            }
            dashCoroutine = StartCoroutine(DashCoroutine(targetPos));
        }

        public override void Activate(Vector3 targetPos)
        {
            base.Activate(targetPos);
            targetPos.y = owner.transform.position.y;
            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
            }
            dashCoroutine = StartCoroutine(DashCoroutine(targetPos));
        }

        public override void Activate<T>(UnitList<T> targets)
        {
            Debug.LogError("Cannot dash to multiple targets! (Source: " + owner.name + ")");
        }

        public override void Tick()
        {
        }

        protected override void OnDeactivated()
        {
            if (dashCoroutine != null)
            {
                StopCoroutine(dashCoroutine);
            }

            StopDashLock();

            if (!isDashing) return;
            var resettedPosition = owner.transform.position;
            resettedPosition.y = 1.5f;
            owner.transform.position = resettedPosition;
        }

        private void ValidateTargetPos(Vector3 _targetPos, out Vector3 validated)
        {
            validated = _targetPos;
            var navMovement = owner.GetComponent<NavMovement>();
            if (!navMovement) return;
            validated = navMovement.ClosestNavigablePos(_targetPos);
        }
    }
}
