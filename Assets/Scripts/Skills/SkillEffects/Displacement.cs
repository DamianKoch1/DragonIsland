using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    //TODO account for distance to source? (modifier that makes targets in center fly further)
    public class Displacement : SkillEffect
    {
        [Space]
        [SerializeField, Range(0.1f, 10)]
        private float duration = 1;

        [SerializeField, Range(0.1f, 10)]
        private float durationPerRank = 1;

        [Space]
        [SerializeField, Range(0, 10)]
        private float maxDistance = 1;

        [SerializeField, Range(0, 10)]
        private float maxDistancePerRank = 1;

        [SerializeField, Tooltip("Time and value should range from 0 to 1")]
        private AnimationCurve distancePerTime = AnimationCurve.Linear(0, 0, 1, 1);

        [Space]
        [SerializeField, Range(0, 5)]
        private float maxHeight;


        [SerializeField, Tooltip("Time and value should range from 0 to 1")]
        private AnimationCurve heightPerTime = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

        private Vector3 direction;

        public override void Activate(Unit _target)
        {
            base.Activate(_target);
            StartCoroutine(DisplacementCoroutine(_target));
        }

        public override void Activate(Vector3 targetPos)
        {
            Debug.LogError("Cannot displace a position! (Source: " + owner.name + ")");
        }

        public override void Activate<T>(UnitList<T> targets)
        {
            foreach (var target in targets)
            {
                Activate(target);
            }
        }

        public override void Tick()
        {
            Debug.LogWarning("Displacements shouldn't tick, repeatedly activate them instead! (Source: " + owner.name + ")");
        }

        protected override void OnDeactivated()
        {
        }

        public override void LevelUp()
        {
            base.LevelUp();
            duration += durationPerRank;
            maxDistance += maxDistancePerRank;
        }

        private IEnumerator DisplacementCoroutine(Unit _target)
        {
            StartDisplacementLock(_target);

            var startPos = _target.transform.position;

            direction = (_target.GetGroundPos() - owner.GetGroundPos()).normalized;

            var targetPos = startPos + direction * maxDistance;
           
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

                _target.transform.position = newPos;

                timePassed += Time.deltaTime;
                yield return null;
            }
            _target.transform.position = targetPos;

            StopDisplacementLock(_target);
        }

        private void StartDisplacementLock(Unit _target)
        {
            _target.CanMove = false;
            if (_target.IsAttacking())
            {
                _target.StopAttacking();
            }
            _target.canAttack = false;
            _target.canCast = false;
        }

        private void StopDisplacementLock(Unit _target)
        {
            _target.canAttack = true;
            _target.canCast = true;
            _target.CanMove = true;
        }
    }
}
