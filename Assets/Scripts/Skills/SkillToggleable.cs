using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    //TODO run into castrange doesn't work for this
    /// <summary>
    /// Can be toggled on and off, consumes resource each tick, activates effects immediately and each tick
    /// </summary>
    public class SkillToggleable : Skill
    {
        [SerializeField, Tooltip("Is true while this toggle / channel is on")]
        private string animatorBool;

        [Space]
        [SerializeField]
        protected bool isToggledOn;

        public bool IsToggledOn => isToggledOn;

        [Space]
        [SerializeField, Range(0, 1000)]
        protected float costPerSec;

        public float CostPerSec => costPerSec;

        [SerializeField, Range(0, 1000)]
        protected float costPerSecPerRank;

        [SerializeField]
        protected bool beginCDOnActivation;

        [SerializeField, Range(-1, 300), Tooltip("Max duration the skill can be toggled on, unlimited if -1, for channel skills use cast time instead of this")]
        protected float maxDuration = -1;

        [SerializeField, Range(0, 300)]
        protected float maxDurationPerRank;

        protected float timeActive;

        [SerializeField, Range(0.1f, 2)]
        private float tickInterval = 0.5f;

        private float timeSinceLastTick;

        /// <summary>
        /// Toggles this skill on, activates effects, starts cooldown here if beginCDOnActivation is enabled
        /// </summary>
        protected virtual void ToggleOn()
        {
            owner.Stats.Resource -= cost;
            isToggledOn = true;
            if (beginCDOnActivation)
            {
                StartCoroutine(StartCooldown());
            }
            timeActive = 0;
            timeSinceLastTick = 0;
            if (castTimeCoroutine != null)
            {
                StopCoroutine(castTimeCoroutine);
            }
            castTimeCoroutine = StartCoroutine(StartCastTime());
            OnCast?.Invoke();
            if (!string.IsNullOrEmpty(animatorTrigger))
            {
                owner.Animator?.SetTrigger(animatorTrigger);
            }
            if (!string.IsNullOrEmpty(animatorBool))
            {
                owner.Animator?.SetBool(animatorBool, true);
            }
            if (lookAtTarget)
            {
                var lookAtPos = mousePosAtCast;
                if (target)
                {
                    lookAtPos = target.transform.position;
                }
                owner.transform.LookAt(new Vector3(lookAtPos.x, owner.transform.position.y, lookAtPos.z));
            }
        }

        /// <summary>
        /// Deactivates all effects, starts cooldown if beginCDOnActivation is disabled
        /// </summary>
        protected virtual void ToggleOff()
        {
            foreach (var effect in effects)
            {
                effect.Deactivate();
            }
            isToggledOn = false;
            OnToggledOff?.Invoke();
            if (!beginCDOnActivation)
            {
                StartCoroutine(StartCooldown());
            }
            if (!string.IsNullOrEmpty(animatorBool))
            {
                owner.Animator?.SetBool(animatorBool, false);
            }
        }

        /// <summary>
        /// Toggles off if toggled on
        /// </summary>
        public override void OnButtonClicked()
        {
            if (!isToggledOn) return;
            ToggleOff();
        }

        public Action OnToggledOff;

        /// <summary>
        /// Toggles off if owner dies, ticks depending on time
        /// </summary>
        protected virtual void Update()
        {
            if (!IsToggledOn) return;
            if (owner.IsDead)
            {
                ToggleOff();
                return;
            }
            timeSinceLastTick += Time.deltaTime;
            while (timeSinceLastTick >= tickInterval)
            {
                timeSinceLastTick -= tickInterval;
                timeActive += tickInterval;
                Tick();
                owner.Stats.Resource -= costPerSec * tickInterval;
                if (maxDuration > 0)
                {
                    if (timeActive >= maxDuration)
                    {
                        ToggleOff();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Increases / decreases costPerSec, maxDuration
        /// </summary>
        public override void LevelUp()
        {
            base.LevelUp();
            if (Rank == 1) return;
            costPerSec += costPerSecPerRank;
            maxDuration += maxDurationPerRank;
        }

        /// <summary>
        /// Tries to toggle on / off instead
        /// </summary>
        /// <param name="hovered">hovered unit</param>
        /// <param name="mousePos">mouse ground position</param>
        /// <returns></returns>
        public override bool TryCast(Unit hovered, Vector3 mousePos)
        {
            if (isToggledOn)
            {
                return TryToggleOff();
            }
            else
            {
                return TryToggleOn(hovered, mousePos);
            }
        }

        /// <summary>
        /// If not in cast time, toggles off
        /// </summary>
        /// <returns></returns>
        protected virtual bool TryToggleOff()
        {
            if (isInCastTime) return false;
            ToggleOff();
            return true;
        }

        /// <summary>
        /// If toggled off, toggles on unless owner can't cast / has too few resource / no valid target
        /// </summary>
        /// <param name="hovered"></param>
        /// <param name="mousePos"></param>
        /// <returns></returns>
        protected virtual bool TryToggleOn(Unit hovered, Vector3 mousePos)
        {
            if (Rank < 1) return false;
            if (!isReady) return false;
            if (!owner.canCast) return false;
            if (owner.Stats.Resource < cost) return false;
            mousePosAtCast = mousePos;
            if (!IsValidTargetSelected(hovered)) return false;
            ToggleOn();
            return true;
        }

        /// <summary>
        /// Ticks all effects, decreases owner cost, toggles off if no valid target / owner / not enough resource
        /// </summary>
        public virtual void Tick()
        {
            if (!isToggledOn) return;
            if (!owner)
            {
                ToggleOff();
                return;
            }
            if (owner.IsDead)
            {
                ToggleOff();
                return;
            }
            if (wasCastOnUnit)
            {
                if (!target)
                {
                    ToggleOff();
                    return;
                }
                if (target.IsDead)
                {
                    ToggleOff();
                    return;
                }
            }
            if (owner.Stats.Resource < costPerSec)
            {
                ToggleOff();
                return;
            }
            foreach (var effect in effects)
            {
                effect.Tick();
            }

        }
    }
}