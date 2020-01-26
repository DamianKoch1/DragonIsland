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

        [SerializeField, Range(-1, 300), Tooltip("Max duration the skill can be toggled on, unlimited if -1")]
        protected float maxDuration = -1;

        protected float timeActive;

        protected void ToggleOn()
        {
            owner.Stats.Resource -= cost;
            isToggledOn = true;
            OnCast?.Invoke();
            if (beginCDOnActivation)
            {
                StartCoroutine(StartCooldown());
            }
            timeActive = 0;
            StartCoroutine(StartCastTime());
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

        public override void SetOwner(Unit _owner)
        {
            base.SetOwner(_owner);
            owner.OnUnitTick += Tick;
        }

        public override bool TryCast()
        {
            if (isToggledOn)
            {
                return TryToggleOff();
            }
            else
            {
                return TryToggleOn();
            }
        }

        protected virtual bool TryToggleOff()
        {
            ToggleOff();
            return true;
        }

        protected virtual bool TryToggleOn()
        {
            if (Rank < 1) return false;
            if (!isReady) return false;
            if (!owner.canCast) return false;
            if (owner.Stats.Resource < cost) return false;
            if (!IsValidTargetSelected()) return false;
            ToggleOn();
            return true;
        }

        public virtual void Tick()
        {
            if (!isToggledOn) return;
            if (owner.Stats.Resource < costPerSec)
            {
                ToggleOff();
                return;
            }
            foreach (var effect in effects)
            {
                effect.Tick();
            }
            owner.Stats.Resource -= costPerSec / Unit.TICKINTERVAL;
            timeActive += Unit.TICKINTERVAL;
            if (maxDuration > 0)
            {
                if (timeActive >= maxDuration)
                {
                    ToggleOff();
                }
            }
        }
    }
}