using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class SkillToggleable : Skill
    {
        [Space]
        [SerializeField]
        protected bool isToggledOn;

        public bool IsToggledOn => isToggledOn;

        [Space]
        [SerializeField, Range(0, 1000)]
        protected float costPerSec;

        public float CostPerSec => costPerSec;

        [SerializeField]
        protected bool beginCDOnActivation;

        [SerializeField, Range(-1, 300), Tooltip("Max duration the skill can be toggled on, unlimited if -1, for channel skills use cast time instead of this")]
        protected float maxDuration = -1;

        protected float timeActive;

        [SerializeField, Range(0.1f, 2)]
        private float tickInterval = 0.5f;

        private float timeSinceLastTick;



        protected void ToggleOn()
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
        }

        protected void ToggleOff()
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
        }


        public override void OnButtonClicked()
        {
            if (!isToggledOn) return;
            ToggleOff();
        }

        public Action OnToggledOff;


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

        protected virtual bool TryToggleOff()
        {
            if (isInCastTime) return false;
            ToggleOff();
            return true;
        }

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