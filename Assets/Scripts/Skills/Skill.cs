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
    /// <summary>
    /// Base class for skills, can simply be activated and goes on cooldown
    /// </summary>
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

        [SerializeField, Range(0, 200)]
        protected float castRangePerRank = 0;

        [SerializeField, Tooltip("This should usually be false, set to true for SpawnProjectile skills that should always be cast towards cursor regardless of distance (only works for mousePos targeting, castRange still relevant for range indicator)")]
        private bool ignoreCastRange = false;

        [Space]
        [SerializeField, Range(0.1f, 300)]
        private float cooldown = 5;

        [SerializeField, Range(0, 100)]
        private float cooldownReductionPerRank;

        [SerializeField, Range(0, 1000)]
        protected float cost = 50;

        public float Cost => cost;

        [SerializeField, Range(0, 1000)]
        protected float costPerRank = 10;


        [Space]
        [SerializeField]
        private TargetingMode targetingMode;

        private Unit prevAttackTarget;

        [SerializeField, Tooltip("The max range closest unit targeting effects use for finding units")]
        private float getClosestUnitRange = 5;

        [SerializeField]
        private bool canCastOnStructures;

        protected bool isReady;

        protected Coroutine castTimeCoroutine;

        protected UnitStats ownerStatsAtCast;

        [Space]
        [SerializeField, Tooltip("Can only be leveled at 6, 11, 16?")]
        private bool isUltimate;

        [SerializeField]
        private int maxRank = 5;

        public bool IsUltimate => isUltimate;

        [SerializeField, Tooltip("Turn to target unit / position on cast?")]
        protected bool lookAtTarget = true;


        [SerializeField, Tooltip("Is set before starting cast time")]
        protected string animatorTrigger;


        public int Rank
        {
            protected set;
            get;
        }

        /// <summary>
        /// Gathers attached effects, setup actions / owner
        /// </summary>
        /// <param name="_owner">owner of this skill</param>
        public virtual void Initialize(Unit _owner)
        {
            isReady = true;
            Rank = 0;
            effects = new List<SkillEffect>(GetComponents<SkillEffect>());
            OnCastTimeFinished += StopCastTimeLock;
            OnCastTimeFinished += ActivateEffects;
            OnCastTimeFinished += () => castTimeCoroutine = null;
            castTimeCoroutine = null;
            SetOwner(_owner);
            owner.OnMovementCommand += CancelMoveIntoCastRange;
        }

        /// <summary>
        /// Sets owner for this and initializes all effects
        /// </summary>
        /// <param name="_owner"></param>
        private void SetOwner(Unit _owner)
        {
            owner = _owner;
            foreach (var effect in effects)
            {
                effect.Initialize(owner, Rank);
            }
        }

        /// <summary>
        /// Increases / decreases castRange, cost, cooldown, levels up effects
        /// </summary>
        public virtual void LevelUp()
        {
            Rank++;
            if (Rank == 1) return;
            cooldown -= cooldownReductionPerRank;
            castRange += castRangePerRank;
            cost += costPerRank;
            foreach (var effect in effects)
            {
                effect.LevelUp();
            }
        }

        /// <summary>
        /// Returns false if no more skill points, already at max rank, if isUltimate returns false if necessary level isn't reached, otherwise returns true
        /// </summary>
        /// <param name="availableSkillPoints">number of currently available skill points to check for</param>
        /// <returns></returns>
        public bool CanBeLeveled(int availableSkillPoints)
        {
            if (availableSkillPoints <= 0)
            {
                return false;
            }
            if (Rank >= maxRank) return false;
            else if (IsUltimate)
            {
                return (Owner.Stats.Lvl - 1) / 5 > Rank;
            }
            return true;
        }

        /// <summary>
        /// Shows range indicator displaying castRange
        /// </summary>
        public virtual void OnMouseEnter()
        {
            ((Champ)owner).ToggleRangeIndicator(true, CastRange);
        }

        /// <summary>
        /// Hides range indicator
        /// </summary>
        public virtual void OnMouseExit()
        {
            ((Champ)owner).ToggleRangeIndicator(false);
        }

        //TODO target selection
        public virtual void OnButtonClicked()
        {

        }

        /// <summary>
        /// Uses owner CDR to calculate actual cooldown, blocks casting again until cooldown has passed
        /// </summary>
        /// <returns></returns>
        protected IEnumerator StartCooldown()
        {
            isReady = false;
            float reducedCooldown = Mathf.Max(cooldown, 0.1f) * 1 - (owner.Stats.CDReduction / 100);
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


        /// <summary>
        /// Stops trying to move into cast range
        /// </summary>
        private void CancelMoveIntoCastRange()
        {
            if (moveIntoCastRange != null)
            {
                StopCoroutine(moveIntoCastRange);
                moveIntoCastRange = null;
                owner.CanMove = true;
            }
        }

        /// <summary>
        /// Tries to cast this skill
        /// </summary>
        /// <param name="hovered"></param>
        /// <param name="mousePos"></param>
        /// <returns>returns false if: currently cooling down / in cast time / not yet learned (rank is 0) / owner can't currently cast / not enough resource / no valid target hovered, otherwise true</returns>
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
                    owner.OnMovementCommand?.Invoke();
                    moveIntoCastRange = StartCoroutine(ChaseOutOfRangeTarget());
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
                owner.OnMovementCommand?.Invoke();
                moveIntoCastRange = StartCoroutine(MoveToOutOfRangePosition());
                return false;
            }
            else
            {
                Cast();
                return true;
            }
        }

        /// <summary>
        /// Moves owner to target until in cast range, then tries to cast this
        /// </summary>
        /// <returns></returns>
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
                    TryCast(target, mousePosAtCast);
                    break;
                }
            }
            moveIntoCastRange = null;
        }

        /// <summary>
        /// Moves owner to position until in cast range, then tries to cast this
        /// </summary>
        /// <returns></returns>
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
                    TryCast(null, mousePosAtCast);
                    break;
                }
            }
            moveIntoCastRange = null;
        }

        /// <summary>
        /// Decreases owner resource by cost, starts cooldown and cast time
        /// </summary>
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
            if (!string.IsNullOrEmpty(animatorTrigger))
            {
                owner.Animator?.SetTrigger(animatorTrigger);
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
        /// Checks if the given unit matches the assigned TargetingMode
        /// </summary>
        /// <param name="hovered">hovered unit</param>
        /// <returns></returns>
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

        /// <summary>
        /// Saves stats at activation (in case e.g. minions can ever cast skills and get destroyed while a projectile flies, also prevents suddenly buying lots of damage), 
        /// does the same for all effects
        /// </summary>
        /// <param name="stats">Stats to save</param>
        public void SetStatsAtActivation(UnitStats stats)
        {
            ownerStatsAtCast = new UnitStats(stats);
            foreach (var effect in effects)
            {
                effect.SetStatsAtActivation(ownerStatsAtCast);
            }
        }

        /// <summary>
        /// Saves stats, starts cast time lock, on finished stops cast time lock and activates effects
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Used to visualize cast time
        /// </summary>
        public Action<float> OnRemainingCastTimeChanged;
        public Action OnCastTimeFinished;

        /// <summary>
        /// Disables casting this / owner attacking / casting (/ movement)
        /// </summary>
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

        /// <summary>
        /// Enables owner attacking / casting (/ movement), resumes attacking previous target / moves to previous destination
        /// </summary>
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

        /// <summary>
        /// Activates each effect considering its own EffectTargetingMode
        /// </summary>
        protected void ActivateEffects()
        {
            foreach (var effect in effects)
            {
                if (Rank < effect.MinRank) continue;
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

        private void OnDrawGizmosSelected()
        {
            if (castRange < 0) return;
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, castRange);
        }
    }

}

