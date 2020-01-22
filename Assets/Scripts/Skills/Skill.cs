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

    public class Skill : MonoBehaviour
    {
        protected Unit owner;

        public Unit Owner => owner;

        [SerializeField]
        private Sprite icon;

        public Sprite Icon => icon;

        [SerializeField]
        protected string skillName;

        protected List<SkillEffect> effects;

        [SerializeField, Range(0.1f, 500)]
        private float cooldown;

        [SerializeField, Min(0)]
        protected float cost;

        [SerializeField, Range(0.1f, 100)]
        protected float castRange;

        [SerializeField]
        private TargetingMode targetingMode;

        [SerializeField]
        private HitMode hitMode;

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
            switch (targetingMode)
            {
                case TargetingMode.mousePos:
                    break;
                case TargetingMode.enemyChamps:
                    break;
                case TargetingMode.enemyUnits:
                    break;
                case TargetingMode.alliedChamps:
                    break;
                case TargetingMode.alliedUnits:
                    break;
                case TargetingMode.monsters:
                    break;
                case TargetingMode.anyUnit:
                    break;
                case TargetingMode.self:
                    break;
                default:
                    Debug.LogError(skillName + " had invalid targetingMode!");
                    break;
            }
            foreach (var effect in effects)
            {
                effect.Activate();
            }
            owner.Stats.Resource -= cost;
            OnCast?.Invoke();
            StartCoroutine(StartCooldown());
            return true;
        }
    }

}

