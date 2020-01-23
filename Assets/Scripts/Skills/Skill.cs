using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public enum TargetingMode
    {
        invalid = -1,
        mousePos = 0,
        enemyChamps = 1,
        enemyUnits = 2,
        alliedChamps = 3,
        alliedUnits = 4,
        monsters = 5,
        anyUnit = 6,
        self = 7
    }


    //TODO silence, cancellable
    public class Skill : MonoBehaviour
    {
        protected Unit owner;

        protected Unit target;

        protected Vector3 mousePos;

        public Unit Owner => owner;

        [SerializeField]
        private Sprite icon;

        public Sprite Icon => icon;

        [SerializeField]
        protected string skillName;

        protected List<SkillEffect> effects;

        [Space]
        [SerializeField, Range(0, 50)]
        private float castTime;

        [SerializeField, Range(0.1f, 300)]
        private float cooldown;

        [SerializeField, Range(0, 300)]
        private float cooldownReductionPerRank;

        [SerializeField, Min(0)]
        protected float cost;

        [SerializeField, Range(-1, 100)]
        protected float castRange;

        [SerializeField]
        private TargetingMode targetingMode;

      

        protected bool isReady;

        public int Rank
        {
            protected set;
            get;
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            isReady = true;
            Rank = 1;
            effects = new List<SkillEffect>(GetComponents<SkillEffect>());
        }

        public virtual void SetOwner(Unit _owner)
        {
            owner = _owner;
            foreach (var effect in effects)
            {
                effect.Initialize(owner, Rank);
            }
        }

        public virtual void OnButtonHovered()
        {

        }

        public virtual void OnButtonClicked()
        {

        }

        protected IEnumerator StartCooldown()
        {
            isReady = false;
            float reducedCooldown = cooldown * 1 - (owner.Stats.CDReduction / 100);
            float remainingTime = reducedCooldown;
            while (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                OnRemainingCDChanged?.Invoke(remainingTime, reducedCooldown);
                yield return null;
            }
            isReady = true;
            OnCDFinished?.Invoke();
        }

        public Action OnCast;
        public Action<float, float> OnRemainingCDChanged;
        public Action OnCDFinished;

        public virtual bool TryCast()
        {
            if (!isReady) return false;
            if (Rank < 1) return false;
            if (owner.Stats.Resource < cost) return false;

            if (!IsValidTargetSelected()) return false;

            owner.Stats.Resource -= cost;
            OnCast?.Invoke();
            StartCoroutine(StartCooldown());
            StartCoroutine(StartCastTime());
            return true;
        }

        protected bool IsValidTargetSelected()
        {
            Unit hovered = PlayerController.Instance.hovered;
            switch (targetingMode)
            {
                case TargetingMode.mousePos:
                    if (!PlayerController.Instance.GetMouseWorldPos(out mousePos)) return false;
                    break;

                case TargetingMode.enemyChamps:
                    if (hovered is Champ == false) return false;
                    if (!owner.IsEnemy(hovered)) return false;
                    target = hovered;
                    return true;

                case TargetingMode.enemyUnits:
                    if (!owner.IsEnemy(hovered)) return false;
                    target = hovered;
                    return true;

                case TargetingMode.alliedChamps:
                    if (hovered is Champ == false) return false;
                    if (!owner.IsAlly(hovered)) return false;
                    target = hovered;
                    return true;

                case TargetingMode.alliedUnits:
                    if (!owner.IsAlly(hovered)) return false;
                    target = hovered;
                    return true;

                case TargetingMode.monsters:
                    if (hovered is Monster == false) return false;
                    target = hovered;
                    return true;

                case TargetingMode.anyUnit:
                    if (!hovered) return false;
                    target = hovered;
                    return true;

                case TargetingMode.self:
                    target = owner;
                    return true;

                default:
                    Debug.LogError(skillName + " had invalid targetingMode!");
                    return false;
            }
            return false;
        }

        protected IEnumerator StartCastTime()
        {
            yield return new WaitForSeconds(castTime);
            ActivateEffects();
        }

        protected void ActivateEffects()
        {
            if (target)
            {
                foreach (var effect in effects)
                {
                    effect.Activate(target);
                }
            }
            else
            {
                foreach (var effect in effects)
                {
                    effect.Activate(mousePos);
                }
            }
        }
    }

}

