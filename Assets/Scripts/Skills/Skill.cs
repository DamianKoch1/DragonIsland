using Photon.Pun;
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


    //TODO silence
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

        [Space]
        [SerializeField, Range(-1, 200), Tooltip("-1 = global")]
        protected float castRange = -1;

        public float CastRange => castRange;

        [SerializeField, Tooltip("This should usually be false, set to true for SpawnProjectile skills that should always be cast towards cursor (only works for mousePos targeting, castRange still relevant for range indicator)")]
        private bool ignoreCastRange = false;

        [Space]
        [SerializeField, Range(0.1f, 300)]
        private float cooldown = 5;

        [SerializeField, Range(0, 100)]
        private float cooldownReductionPerRank;

        [SerializeField, Range(0, 1000)]
        protected float cost = 50;

        public float Cost => cost;


        [Space]
        [SerializeField]
        private TargetingMode targetingMode;

        private Unit prevAttackTarget;

        [SerializeField, Tooltip("Ignore this if targeting mode is inherit, find range for closest units to be targeted instead")]
        private float getClosestUnitRange = 5;

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
        public virtual void OnMouseEnter()
        {
            ((Champ)owner).ToggleRangeIndicator(true, CastRange);
        }

        public virtual void OnMouseExit()
        {
            ((Champ)owner).ToggleRangeIndicator(false);
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

        private Coroutine moveIntoCastRange;

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
                if (Vector3.Distance(owner.GetGroundPos(), target.GetGroundPos()) > castRange)
                {
                    if (moveIntoCastRange != null)
                    {
                        StopCoroutine(moveIntoCastRange);
                    }
                    StartCoroutine(ChaseOutOfRangeTarget());
                    return false;
                }
                else
                {
                    Cast();
                    return true;
                }
            }
            else if (Vector3.Distance(owner.GetGroundPos(), mousePosAtCast) > castRange)
            {
                if (ignoreCastRange)
                {
                    Cast();
                    return true;
                }
                else if (moveIntoCastRange != null)
                {
                    StopCoroutine(moveIntoCastRange);
                }
                StartCoroutine(MoveToOutOfRangePosition());
                return false;
            }
            else
            {
                Cast();
                return true;
            }
        }

        private IEnumerator ChaseOutOfRangeTarget()
        {
            while (true)
            {
                if (!target)
                {
                    owner.Stop();
                    break;
                }
                if (target.IsDead)
                {
                    owner.Stop();
                    break;
                }
                if (owner.IsDead)
                {
                    owner.Stop();
                    break;
                }
                if (!owner.canCast)
                {
                    owner.Stop();
                    break;
                }
                if (Vector3.Distance(owner.GetGroundPos(), target.GetGroundPos()) > CastRange)
                {
                    owner.CanMove = true;
                    owner.MoveTo(target.GetGroundPos());
                    yield return null;
                    continue;
                }
                else
                {
                    Cast();
                    break;
                }
            }
        }

        private IEnumerator MoveToOutOfRangePosition()
        {
            while (true)
            {
                if (owner.IsDead)
                {
                    owner.Stop();
                    break;
                }
                if (!owner.canCast)
                {
                    owner.Stop();
                    break;
                }
                if (Vector3.Distance(owner.GetGroundPos(), mousePosAtCast) > CastRange)
                {
                    owner.CanMove = true;
                    owner.MoveTo(mousePosAtCast);
                    yield return null;
                    continue;
                }
                else
                {
                    Cast();
                    break;
                }
            }
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
            foreach (var effect in effects)
            {
                switch (effect.TargetingMode)
                {
                    case EffectTargetingMode.inherit:
                        if (target)
                        {
                            effect.Activate(target);
                        }
                        else
                        {
                            effect.Activate(mousePosAtCast);
                        }
                        break;
                    case EffectTargetingMode.closestUnit:
                        var targets = owner.GetUnitsInRange<Unit>(getClosestUnitRange, canCastOnStructures);
                        if (targets.Count() == 0) continue;
                        var closest = owner.GetClosestUnit(targets);
                        effect.Activate(closest);
                        break;
                    case EffectTargetingMode.closestAlly:
                        targets = owner.GetUnitsInRange<Unit>(getClosestUnitRange, canCastOnStructures).FindAllies(owner);
                        if (targets.Count() == 0) continue;
                        closest = owner.GetClosestUnit(targets);
                        effect.Activate(closest);
                        break;
                    case EffectTargetingMode.closestEnemy:
                        targets = owner.GetUnitsInRange<Unit>(getClosestUnitRange, canCastOnStructures).FindEnemies(owner);
                        if (targets.Count() == 0) continue;
                        closest = owner.GetClosestUnit(targets);
                        effect.Activate(closest);
                        break;
                    case EffectTargetingMode.closestAllyChamp:
                        var targets1 = owner.GetUnitsInRange<Champ>(getClosestUnitRange, canCastOnStructures).FindAllies(owner);
                        if (targets1.Count() == 0) continue;
                        closest = owner.GetClosestUnit(targets1);
                        effect.Activate(closest);
                        break;
                    case EffectTargetingMode.closestEnemyChamp:
                        targets1 = owner.GetUnitsInRange<Champ>(getClosestUnitRange, canCastOnStructures).FindEnemies(owner);
                        if (targets1.Count() == 0) continue;
                        closest = owner.GetClosestUnit(targets1);
                        effect.Activate(closest);
                        break;
                    case EffectTargetingMode.self:
                        effect.Activate(owner);
                        break;
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

