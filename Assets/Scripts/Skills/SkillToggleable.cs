using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class SkillToggleable : Skill
    {

        [SerializeField]
        private bool isToggledOn;

        [SerializeField, Range(0, 1000)]
        private float tickCost;

        [SerializeField]
        private bool beginCDOnActivation;

        private void OnToggledOn()
        {
            owner.Stats.Resource -= cost;
            foreach (var effect in effects)
            {
                effect.Activate();
            }
            isToggledOn = true;
            if (beginCDOnActivation)
            {
                StartCoroutine(StartCooldown());
            }
            print(owner.name + " activated " + skillName);
        }

        private void OnToggledOff()
        {
            foreach (var effect in effects)
            {
                effect.Deactivate();
            }
            isToggledOn = false;
            if (!beginCDOnActivation)
            {
                StartCoroutine(StartCooldown());
            }
            print(owner.name + " deactivated " + skillName);
        }

        public override void Initialize(Unit _owner)
        {
            base.Initialize(_owner);
            owner.OnUnitTick += Tick;
        }

        public override bool TryCast()
        {
            if (rank < 1) return false;
            if (isToggledOn)
            {
                OnToggledOff();
                return true;
            }
            else
            {
                if (owner.Stats.Resource < cost) return false;
                if (!isReady) return false;
                OnToggledOn();
                return false;
            }
        }

        public virtual void Tick()
        {
            if (!isToggledOn) return;
            if (owner.Stats.Resource < tickCost)
            {
                OnToggledOff();
                return;
            }
            foreach (var effect in effects)
            {
                effect.Tick();
            }
            owner.Stats.Resource -= tickCost;
            print(owner.name + " tick " + skillName);
        }
    }
}