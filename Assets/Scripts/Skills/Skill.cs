﻿using Photon.Pun;
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


    //TODO silence, chase until in castrange
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PhotonView))]

    public class Skill : MonoBehaviour
    {
        protected Unit owner;

        protected Unit target;

        protected bool wasCastOnUnit;

        protected Vector3 mousePosAtCast;

        public Unit Owner => owner;

        [SerializeField]
        private Sprite icon;

        public Sprite Icon => icon;

        [SerializeField]
        protected string skillName;

        protected List<SkillEffect> effects;

        protected bool isInCastTime;

        [Space]
        [SerializeField, Range(0, 50)]
        protected float castTime;

        [SerializeField]
        private bool canMoveWhileCasting;

        public float CastTime => castTime;

        [SerializeField, Range(0.1f, 300)]
        private float cooldown = 5;

        [SerializeField, Range(0, 300)]
        private float cooldownReductionPerRank;

        [SerializeField, Min(0)]
        protected float cost = 50;

        public float Cost => cost;

        [SerializeField, Range(-1, 200)]
        protected float castRange = -1;

        [SerializeField]
        private TargetingMode targetingMode;

        private Unit prevAttackTarget;

        [SerializeField]
        private bool canCastOnStructures;

        protected bool isReady;

        protected Coroutine castTimeCoroutine;

        protected UnitStats ownerStatsAtCast;

        public int Rank
        {
            protected set;
            get;
        }

        public virtual void Initialize(Unit _owner)
        {
            isReady = true;
            Rank = 1;
            effects = new List<SkillEffect>(GetComponents<SkillEffect>());
            OnCastTimeFinished += StopCastTimeLock;
            OnCastTimeFinished += ActivateEffects;
            OnCastTimeFinished += () => castTimeCoroutine = null;
            castTimeCoroutine = null;
            SetOwner(_owner);
        }

        private void SetOwner(Unit _owner)
        {
            owner = _owner;
            foreach (var effect in effects)
            {
                effect.Initialize(owner, Rank);
            }
        }


        //TODO target selection
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

        public virtual bool TryCast(Unit hovered, Vector3 mousePos)
        {
            if (!isReady) return false;
            if (Rank < 1) return false;
            if (!owner.canCast) return false;
            if (owner.Stats.Resource < cost) return false;

            mousePosAtCast = mousePos;
            if (!IsValidTargetSelected(hovered)) return false;

            if (castRange < 0)
            {
                Cast();
                return true;
            }
            else if (target)
            {
                if (Vector3.Distance(owner.GetGroundPos(), target.GetGroundPos()) <= castRange)
                {
                    Cast();
                    return true;
                }
            }
            else if (Vector3.Distance(owner.GetGroundPos(), mousePosAtCast) <= castRange)
            {
                Cast();
                return true;
            }
            return false;
        }

        private void Cast()
        {
            owner.Stats.Resource -= cost;
            StartCoroutine(StartCooldown());
            if (castTimeCoroutine != null)
            {
                StopCoroutine(castTimeCoroutine);
            }
            castTimeCoroutine = StartCoroutine(StartCastTime());
            OnCast?.Invoke();
        }

        protected bool IsValidTargetSelected(Unit hovered)
        {
            wasCastOnUnit = true;
            switch (targetingMode)
            {
                case TargetingMode.mousePos:
                    wasCastOnUnit = false;
                    return true;

                case TargetingMode.enemyChamps:
                    if (hovered is Champ == false) return false;
                    if (!owner.IsEnemy(hovered)) return false;
                    target = hovered;
                    return true;

                case TargetingMode.enemyUnits:
                    if (!hovered) return false;
                    if (!owner.IsEnemy(hovered)) return false;
                    if (hovered is Structure && !canCastOnStructures) return false;
                    target = hovered;
                    return true;

                case TargetingMode.alliedChamps:
                    if (hovered is Champ == false) return false;
                    if (!owner.IsAlly(hovered)) return false;
                    target = hovered;
                    return true;

                case TargetingMode.alliedUnits:
                    if (!hovered) return false;
                    if (!owner.IsAlly(hovered)) return false;
                    if (hovered is Structure && !canCastOnStructures) return false;
                    target = hovered;
                    return true;

                case TargetingMode.monsters:
                    if (hovered is Monster == false) return false;
                    target = hovered;
                    return true;

                case TargetingMode.anyUnit:
                    if (!hovered) return false;
                    if (hovered is Structure && !canCastOnStructures) return false;
                    target = hovered;
                    return true;

                case TargetingMode.self:
                    target = owner;
                    return true;

                default:
                    Debug.LogError(skillName + " had invalid targetingMode!");
                    return false;
            }
        }

        public void SetStatsAtActivation(UnitStats stats)
        {
            ownerStatsAtCast = new UnitStats(stats);
            foreach (var effect in effects)
            {
                effect.ownerStatsAtActivation = ownerStatsAtCast;
            }
        }

        protected IEnumerator StartCastTime()
        {
            SetStatsAtActivation(owner.Stats);

            if (castTime > 0)
            {
                StartCastTimeLock();

                float remainingTime = castTime;
                while (remainingTime > 0)
                {
                    remainingTime -= Time.deltaTime;
                    OnRemainingCastTimeChanged?.Invoke(remainingTime);
                    yield return null;
                }
            }
            OnCastTimeFinished?.Invoke();
        }

        public Action<float> OnRemainingCastTimeChanged;
        public Action OnCastTimeFinished;

        private void StartCastTimeLock()
        {
            prevAttackTarget = null;
            isInCastTime = true;

            if (owner.IsAttacking())
            {
                prevAttackTarget = owner.CurrentAttackTarget;
                owner.StopAttacking();
            }
            owner.canAttack = false;
            owner.canCast = false;
            if (!canMoveWhileCasting)
            {
                owner.CanMove = false;
            }
        }

        protected void StopCastTimeLock()
        {
            isInCastTime = false;
            owner.canAttack = true;
            owner.canCast = true;
            if (!canMoveWhileCasting)
            {
                owner.CanMove = true;
            }
            if (owner.IsDead) return;
            if (prevAttackTarget)
            {
                owner.StartAttacking(prevAttackTarget);
            }
            else if (owner.GetDestination() != Vector3.zero)
            {
                owner.MoveTo(owner.GetDestination());
            }
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
                    effect.Activate(mousePosAtCast);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (castRange < 0) return;
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, castRange);
        }
    }

}

