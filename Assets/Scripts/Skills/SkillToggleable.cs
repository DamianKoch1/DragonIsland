using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class SkillToggleable : Skill
    {

        [SerializeField]
        private bool isToggledOn;

        public bool IsToggledOn => isToggledOn;

        [SerializeField, Range(0, 1000)]
        private float costPerSec;

        public float CostPerSec => costPerSec;

        [SerializeField]
        private bool beginCDOnActivation;

        [SerializeField, Range(-1, 300)]
        private float maxDuration = -1;

        private float timeActive;

        private void ToggleOn()
        {
            owner.Stats.Resource -= cost;
            foreach (var effect in effects)
            {
                effect.Activate();
            }
            isToggledOn = true;
            OnCast?.Invoke();
            if (beginCDOnActivation)
            {
                StartCoroutine(StartCooldown());
            }
            timeActive = 0;
        }

        private void ToggleOff()
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

        public Action OnToggledOff;

        public override void SetOwner(Unit _owner)
        {
            base.SetOwner(_owner);
            owner.OnUnitTick += Tick;
        }

        public override bool TryCast()
        {
            if (Rank < 1) return false;
            if (isToggledOn)
            {
                ToggleOff();
                return true;
            }
            else
            {
                if (owner.Stats.Resource < cost) return false;
                if (!isReady) return false;
                ToggleOn();
                return false;
            }
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