using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    /// <summary>
    /// Used to instantly teleport owner unit somewhere
    /// </summary>
    public class Teleport : SkillEffect
    {
        [SerializeField, Tooltip("If true, will teleport to mouse position each tick, only works on Champs")]
        private bool canTick = false;

        public override void Activate<T>(UnitList<T> targets)
        {
            Debug.LogError("Cannot teleport to multiple targets! (Source: " + owner.name + ")");
        }

        /// <summary>
        /// Teleports close to target unit
        /// </summary>
        /// <param name="_target"></param>
        public override void Activate(Unit _target)
        {
            base.Activate(_target);
            var targetPos = _target.GetGroundPos();
            targetPos -= (targetPos - owner.GetGroundPos()).normalized;
            ValidateTargetPos(targetPos, out targetPos);
            targetPos.y = owner.transform.position.y;
            owner.Teleport(targetPos);
        }

        /// <summary>
        /// Teleports directly to targetPos
        /// </summary>
        /// <param name="targetPos"></param>
        public override void Activate(Vector3 targetPos)
        {
            base.Activate(targetPos);
            ValidateTargetPos(targetPos, out targetPos);
            targetPos.y = owner.transform.position.y;
            owner.Teleport(targetPos);
        }

        /// <summary>
        /// Teleports to mouse ground position if canTick
        /// </summary>
        public override void Tick()
        {
            if (!(owner is Champ)) return;
            if (!canTick) return;
            PlayerController.Instance.GetMouseWorldPos(out var mousePos);
            Activate(mousePos);
        }


        protected override void OnDeactivated()
        {
        }
    }
}
