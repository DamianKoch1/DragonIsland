using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    //TODO cancellable by moving
    public class SkillChannel : SkillToggleable
    {
        [Space]
        [SerializeField]
        private bool cancellable;

        [SerializeField]
        private bool lookAtCursor;

        [SerializeField, Range(0, 360), Tooltip("Degrees per second")]
        private float turnRate = 20;

        public override void Initialize(Unit _owner)
        {
            base.Initialize(_owner);
            OnCast += ActivateEffects;
            OnCastTimeFinished -= ActivateEffects;
            OnCastTimeFinished += ToggleOff;
        }

        protected override void Update()
        {
            base.Update();
            if (!IsToggledOn) return;
            if (!lookAtCursor) return;
            if (PlayerController.Instance.GetMouseWorldPos(out var mousePos))
            {
                mousePos.y = owner.transform.position.y;
                var newForward = Vector3.RotateTowards(owner.transform.forward, mousePos - owner.GetGroundPos(), turnRate * Mathf.Deg2Rad * Time.deltaTime, 0);
                newForward.y = 0;
                owner.transform.forward = newForward;
            }
        }

        protected override void ToggleOn()
        {
            if (lookAtCursor)
            {
                owner.transform.LookAt(mousePosAtCast);
            }
            base.ToggleOn();
        }

        protected override bool TryToggleOff()
        {
            if (!cancellable) return false;
            if (castTimeCoroutine != null)
            {
                StopCoroutine(castTimeCoroutine);
            }
            OnCastTimeFinished?.Invoke();
            return true;
        }
    }
}
