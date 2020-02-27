using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    //TODO cancellable by moving
    /// <summary>
    /// Similar to a toggle, activates effects immediately and each tick during castTime, doesn't use maxDuration
    /// </summary>
    public class SkillChannel : SkillToggleable
    {
        [Space]
        [SerializeField]
        private bool cancellable;

        [SerializeField]
        private bool lookAtCursor;

        [SerializeField, Range(0, 360), Tooltip("Degrees per second")]
        private float turnRate = 20;

        [SerializeField, Range(0, 360)]
        private float turnRatePerRank = 10;

        [Space]
        [SerializeField, Range(-1, 4)]
        private float cameraZoom = -1;
        private float prevZoom;
        private bool wasCamUnlocked;

        /// <summary>
        /// Sets up events
        /// </summary>
        /// <param name="_owner"></param>
        public override void Initialize(Unit _owner)
        {
            base.Initialize(_owner);
            OnCast += ActivateEffects;
            OnCastTimeFinished -= ActivateEffects;
            OnCastTimeFinished += ToggleOff;
        }

        /// <summary>
        /// Increases turnRate
        /// </summary>
        public override void LevelUp()
        {
            base.LevelUp();
            if (Rank == 1) return;
            turnRate += turnRatePerRank;
        }

        /// <summary>
        /// Turn towards mouse ground position using turnRate if lookAtCursor is enabled
        /// </summary>
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

        /// <summary>
        /// Zooms camera out if cameraZoom is positive, starts channel
        /// </summary>
        protected override void ToggleOn()
        {
            if (lookAtCursor)
            {
                owner.transform.LookAt(mousePosAtCast);
            }
            if (cameraZoom > 0)
            {
                var cam = ChampCamera.Instance;
                wasCamUnlocked = cam.Unlocked;
                prevZoom = cam.CurrentZoom;
                cam.Lock();
                cam.SetZoom(cameraZoom);
                cam.DisableControls();
            }
            base.ToggleOn();
        }

        /// <summary>
        /// Stops channel, resets camera
        /// </summary>
        protected override void ToggleOff()
        {
            base.ToggleOff();
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
        }

        /// <summary>
        /// Doesn't work if not cancellable
        /// </summary>
        /// <returns></returns>
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
