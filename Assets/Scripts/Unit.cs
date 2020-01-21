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

    public abstract class Unit : MonoBehaviour
    {

        [SerializeField]
        private TeamID teamID = TeamID.invalid;

        public TeamID TeamID => teamID;




        private List<Material> defaultMaterials;
        private List<Material> outlineMaterials;
        private List<Renderer> renderers;

       

        [HideInInspector]
        public Amplifiers amplifiers;

        [Space]

        [SerializeField]
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


        private bool isDead;

        public bool IsDead
        {
            protected set
            {
                isDead = value;
            }
            get => isDead;
        }


        protected virtual float GetXPNeededForLevel(int level)
        {
            return (level - 1) * (level - 1) * 100;
        }

        public void LevelUp()
        {
            XP = GetXPNeededForLevel(Lvl + 1);
        }

        public Action<int> OnLevelUp;

        protected virtual void LevelUpStats(int level)
        {
            Lvl = level;

            MaxHP += HPPerLvl;
            HP += HPPerLvl;
            HPReg += HPRegPerLvl;

            MaxResource += resourcePerLvl;
            Resource += resourcePerLvl;
            ResourceReg += resourceRegPerLvl;

            AtkDmg += atkDmgPerLvl;
            AtkSpeed += atkSpeedPerLvl;

            Armor += armorPerLvl;
            MagicRes += magicResPerLvl;
        }

        public void UpdateStats()
        {
            //items + buffs + base
        }

        public Vector3 GetGroundPos()
        {
            return new Vector3(transform.position.x, 0, transform.position.z);
        }

        public UnitList<T> GetTargetableEnemiesInAtkRange<T>(Vector3 fromPosition) where T : Unit
        {
            UnitList<T> result = new UnitList<T>();
            foreach (var collider in Physics.OverlapSphere(fromPosition, AtkRange))
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
            HP -= amount;
            instigator.OnDealDamage?.Invoke(this, amount, type);
            if (instigator is Champ)
            {
                OnAttackedByChamp?.Invoke((Champ)instigator);
            }
            if (HP == 0)
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
                champ.XP += GetXPReward() / xpEligibleChamps.Count();
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

            OnLevelUp += LevelUpStats;
            Lvl = 1;
            XP = 0;

            HP = baseHP;
            MaxHP = baseHP;
            HPReg = baseHPReg;

            Resource = baseResource;
            MaxResource = baseResource;
            ResourceReg = baseResourceReg;

            AtkDmg = baseAtkDmg;
            AtkSpeed = baseAtkSpeed;
            MagicDmg = 0;

            Armor = baseArmor;
            MagicRes = baseMagicRes;

            FlatArmorPen = 0;
            PercentArmorPen = 0;

            FlatMagicPen = 0;
            PercentMagicPen = 0;

            CDReduction = 0;
            CritChance = 0;

            timeSinceLastRegTick = 0;

            amplifiers = new Amplifiers();
            amplifiers.Initialize();

            movement?.Initialize(moveSpeed);
            OnMoveSpeedChanged?.Invoke(moveSpeed);
            attacking?.Initialize(this);

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
            while (timeSinceLastRegTick >= 0.5f)
            {
                ApplyRegeneration();
                timeSinceLastRegTick--;
            }
            timeSinceLastRegTick += Time.deltaTime;
        }

        protected virtual void ApplyRegeneration()
        {
            if (IsDead) return;
            ApplyHPReg();
            ApplyResourceReg();
        }

        protected void ApplyHPReg()
        {
            if (HP >= MaxHP) return;
            HP = Mathf.Min(MaxHP, HP + HPReg);
        }

        protected void ApplyResourceReg()
        {
            if (Resource >= MaxResource) return;
            Resource = Mathf.Min(MaxResource, Resource + ResourceReg);
        }


        public abstract float GetXPReward();

        public abstract int GetGoldReward();

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }

        public bool IsEnemy(Unit other)
        {
            if (teamID == TeamID.blue)
            {
                if (other.teamID == TeamID.red)
                {
                    return true;
                }
            }
            if (teamID == TeamID.red)
            {
                if (other.teamID == TeamID.blue)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsAlly(Unit other)
        {
            if (teamID == TeamID.blue)
            {
                if (other.teamID == TeamID.blue)
                {
                    return true;
                }
            }
            if (teamID == TeamID.red)
            {
                if (other.teamID == TeamID.red)
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
                attacking.AtkTrigger.radius = atkRange;
            }
            if (attacking?.RangeIndicator)
            {
                attacking.RangeIndicator.localScale = new Vector3(atkRange, atkRange, atkRange);
            }
        }
    }
}