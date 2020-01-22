using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MOBA
{
    public enum TeamID
    {
        invalid = -1,
        blue = 0,
        red = 1,
        neutral = 2,
        passive = 3,
    }

    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Unit : MonoBehaviour
    {
        public const float TICKINTERVAL = 0.5f;

        [SerializeField]
        private TeamID teamID = TeamID.invalid;

        public TeamID TeamID => teamID;


        [SerializeField]
        protected UnitStats stats;

        public UnitStats Stats => stats;



        private List<Material> defaultMaterials;
        private List<Material> outlineMaterials;
        private List<Renderer> renderers;



        [HideInInspector]
        public Amplifiers amplifiers;

        [Space]
        private bool canMove = true;

        public bool CanMove
        {
            set
            {
                if (!movement) return;
                if (canMove != value)
                {
                    if (value)
                    {
                        movement.Enable();
                    }
                    else
                    {
                        movement.Disable();
                    }
                }
                canMove = value;
            }
            get => canMove;
        }

        [SerializeField]
        private bool targetable = true;

        public bool Targetable
        {
            set
            {
                if (targetable != value)
                {
                    if (value)
                    {
                        if (PlayerController.Instance.hovered == this)
                        {
                            ShowOutlines();
                        }
                        OnBecomeTargetable?.Invoke();
                    }
                    else
                    {
                        if (PlayerController.Instance.hovered == this)
                        {
                            HideOutlines();
                        }
                        OnBecomeUntargetable?.Invoke();
                    }
                }
                targetable = value;
            }
            get => targetable;
        }

        public Action OnBecomeUntargetable;
        public Action OnBecomeTargetable;

        public bool damageable = true;

        [HideInInspector]
        public bool canAttack = true;

        [Space]

        /// <summary>
        /// If attacker is closer than its own atkRange + this units radius, it can attack this unit. Yellow gizmo wire sphere should include units horizontal bounds, height doesn't matter. Used to prevent not attacking until unit middle is in atkRange
        /// </summary>
        [SerializeField]
        private float radius;

        public float Radius => radius;

        protected float timeSinceLastRegTick = 0;

    

        [SerializeField]
        private float xpRewardRange = 12;

        [Space]

        [SerializeField]
        protected Movement movement;

        [SerializeField]
        protected Attacking attacking;

        [SerializeField]
        protected GameObject statBarsPrefab;

        protected GameObject statBarsInstance;

        [Space]

        [SerializeField]
        protected GameObject mesh;

        public bool IsDead { protected set; get; }


        public virtual float GetXPNeededForLevel(int level)
        {
            return (level - 1) * (level - 1) * 100;
        }

        public void LevelUp()
        {
            stats.LevelUp();
        }


        //TODO
        public void UpdateStats()
        {
            //items + buffs + base + perLvl * (lvl-1)
        }

        public Vector3 GetGroundPos()
        {
            return new Vector3(transform.position.x, 0, transform.position.z);
        }

        public UnitList<T> GetTargetableEnemiesInAtkRange<T>(Vector3 fromPosition) where T : Unit
        {
            UnitList<T> result = new UnitList<T>();
            foreach (var collider in Physics.OverlapSphere(fromPosition, stats.AtkRange))
            {
                if (collider.isTrigger) continue;
                var unit = collider.GetComponent<T>();
                if (!unit) continue;
                if (!IsEnemy(unit)) continue;
                if (!unit.Targetable) continue;
                result.Add(unit);
            }
            return result;
        }

        public UnitList<T> GetEnemiesInRange<T>(float range) where T : Unit
        {
            UnitList<T> result = new UnitList<T>();
            foreach (var collider in Physics.OverlapSphere(transform.position, range))
            {
                if (collider.isTrigger) continue;
                var unit = collider.GetComponent<T>();
                if (!unit) continue;
                if (!IsEnemy(unit)) continue;
                result.Add(unit);
            }
            return result;
        }


        protected Unit GetClosestUnit<T>(UnitList<T> fromList) where T : Unit
        {
            if (fromList.Count() == 0) return null;
            float lowestDistance = Mathf.Infinity;
            Unit closestUnit = null;
            foreach (var unit in fromList)
            {
                float distance = Vector3.Distance(transform.position, unit.transform.position);
                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                    closestUnit = unit;
                }
            }
            return closestUnit;
        }


        /// <summary>
        /// Avoid calling this directly, create a new Damage() and use Inflict() on it.
        /// </summary>
        /// <param name="instigator"></param>
        /// <param name="amount"></param>
        /// <param name="type"></param>
        public virtual void ReceiveDamage(Unit instigator, float amount, DamageType type)
        {
            if (!damageable) return;
            OnReceiveDamage?.Invoke(instigator, amount, type);
            stats.HP -= amount;
            if (instigator)
            {
                instigator.OnDealDamage?.Invoke(this, amount, type);
                if (instigator is Champ)
                {
                    OnAttackedByChamp?.Invoke((Champ)instigator);
                }
            }
            if (stats.HP == 0)
            {
                Die(instigator);
            }
        }

        public Action<Unit, float, DamageType> OnReceiveDamage;

        public Action<Champ> OnAttackedByChamp;

        protected virtual void Die(Unit killer)
        {
            var xpEligibleChamps = GetEnemiesInRange<Champ>(xpRewardRange);
            if (killer is Champ)
            {
                var champ = (Champ)killer;
                champ.Gold += GetGoldReward();
                if (!xpEligibleChamps.Contains(champ))
                {
                    xpEligibleChamps.Add(champ);
                }
            }
            foreach (var champ in xpEligibleChamps)
            {
                champ.stats.XP += GetXPReward() / xpEligibleChamps.Count();
            }
            OnBeforeDeath?.Invoke();
            IsDead = true;
            OnDeath();
        }

        /// <summary>
        /// Is called just before OnDeath(), which destroys this game object by default.
        /// </summary>
        public Action OnBeforeDeath;

        /// <summary>
        /// Destroys this gameObject unless overridden.
        /// </summary>
        protected virtual void OnDeath()
        {
            Destroy(gameObject);
        }

        public Action<Unit, float, DamageType> OnDealDamage;


        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            SetupBars();

            IsDead = false;


            stats.Initialize(this);

            timeSinceLastRegTick = 0;

            amplifiers = new Amplifiers();
            amplifiers.Initialize();

            if (movement)
            {
                stats.OnMoveSpeedChanged += movement.SetSpeed;
            }

            movement?.Initialize(stats.MoveSpeed);

            attacking?.Initialize(this);

            OnUnitTick += ApplyRegeneration;

            SetupMaterials();
        }

        protected virtual void SetupBars()
        {
            statBarsInstance = Instantiate(statBarsPrefab);
            statBarsInstance.GetComponent<UnitStatBars>()?.Initialize(this);
        }

        protected void SetupMaterials()
        {
            defaultMaterials = new List<Material>();
            outlineMaterials = new List<Material>();
            renderers = new List<Renderer>();
            var outlineColor = GetOutlineColor();

            foreach (var renderer in mesh.GetComponentsInChildren<Renderer>())
            {
                renderers.Add(renderer);
                defaultMaterials.Add(new Material(renderer.material));

                var outlineMaterial = new Material(renderer.material);
                outlineMaterial.shader = PlayerController.Instance.outline;
                outlineMaterial.SetColor("_OutlineColor", GetOutlineColor());
                outlineMaterial.SetFloat("_Outline", 0.25f);
                outlineMaterials.Add(outlineMaterial);
            }
        }



        protected virtual Color GetOutlineColor()
        {
            if (IsAlly(PlayerController.Player))
            {
                return PlayerController.Instance.defaultColors.allyOutline;
            }
            return PlayerController.Instance.defaultColors.enemyOutline;
        }

        public virtual Color GetHPColor()
        {
            if (IsAlly(PlayerController.Player))
            {
                return PlayerController.Instance.defaultColors.allyMinionHP;
            }
            return PlayerController.Instance.defaultColors.enemyMinionHP;
        }

        private void OnMouseEnter()
        {
            PlayerController.Instance.hovered = this;
            if (!Targetable)
            {
                if (!IsAlly(PlayerController.Player)) return;
            }
            ShowOutlines();
        }

        protected virtual void ShowOutlines()
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].material = outlineMaterials[i];
            }
        }

        protected virtual void HideOutlines()
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                renderers[i].material = defaultMaterials[i];
            }
        }


        private void OnMouseExit()
        {
            if (PlayerController.Instance.hovered == this)
            {
                PlayerController.Instance.hovered = null;
                HideOutlines();
            }
        }


        public void MoveTo(Vector3 destination)
        {
            if (!CanMove) return;
            if (!movement) return;
            movement.MoveTo(destination);
        }
        protected virtual void Update()
        {
            while (timeSinceLastRegTick >= TICKINTERVAL)
            {
                OnUnitTick?.Invoke();
                timeSinceLastRegTick = 0;
            }
            timeSinceLastRegTick += Time.deltaTime;
        }

        public Action OnUnitTick;

        protected virtual void ApplyRegeneration()
        {
            if (IsDead) return;
            stats.ApplyHPReg();
            stats.ApplyResourceReg();
        }




        public virtual float GetXPReward()
        {
            return 0;
        }

        public virtual int GetGoldReward()
        {
            return 0;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stats.AtkRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        public bool IsEnemy(Unit other)
        {
            return IsEnemy(other.teamID);
        }

        public bool IsEnemy(TeamID id)
        {
            if (teamID == TeamID.blue)
            {
                if (id == TeamID.red)
                {
                    return true;
                }
            }
            if (teamID == TeamID.red)
            {
                if (id == TeamID.blue)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsAlly(Unit other)
        {
            return IsAlly(other.teamID);
        }

        public bool IsAlly(TeamID id)
        {
            if (teamID == TeamID.blue)
            {
                if (id == TeamID.blue)
                {
                    return true;
                }
            }
            if (teamID == TeamID.red)
            {
                if (id == TeamID.red)
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual void OnValidate()
        {
            if (attacking?.AtkTrigger)
            {
                attacking.AtkTrigger.radius = stats.AtkRange;
            }
            if (attacking?.RangeIndicator)
            {
                attacking.RangeIndicator.localScale = new Vector3(stats.AtkRange, 1, stats.AtkRange);
            }
        }
    }
}