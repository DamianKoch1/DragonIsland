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
        protected TeamID teamID = TeamID.invalid;

        public TeamID TeamID => teamID;

        [SerializeField]
        protected int maxLvl = 18;

        public int Lvl
        {
            get;
            protected set;
        }

        protected float xp = 0;

        [SerializeField]
        protected GameObject mesh;

        protected List<Material> defaultMaterials;
        protected List<Material> outlineMaterials;
        protected List<Renderer> renderers;

        public float XP
        {
            set
            {
                if (Lvl >= maxLvl) return;
                xp = value;
                if (xp == 0) return;
                if (xp >= GetXPNeededForLevel(Lvl + 1))
                {
                    OnLevelUp?.Invoke(Lvl + 1);
                }
                OnXPChanged?.Invoke(xp, GetXPNeededForLevel(Lvl + 1));
            }
            get
            {
                return xp;
            }
        }

        public Action<float, float> OnXPChanged;

        [SerializeField]
        protected float baseHP;
        public float MaxHP
        {
            protected set;
            get;
        }
        protected float hp;
        public float HP
        {
            protected set
            {
                hp = Mathf.Max(0, value);
                OnHPChanged?.Invoke(hp, MaxHP);
            }
            get
            {
                return hp;
            }
        }
        public Action<float, float> OnHPChanged;


        [SerializeField]
        protected float HPPerLvl;

        [SerializeField]
        protected float baseHPReg;
        public float HPReg
        {
            protected set;
            get;
        }
        [SerializeField]
        protected float HPRegPerLvl;


        [SerializeField]
        protected float baseResource;



        public float MaxResource
        {
            protected set;
            get;
        }
        protected float resource;
        public float Resource
        {
            protected set
            {
                resource = value;
                OnResourceChanged?.Invoke(Resource, MaxResource);
            }
            get
            {
                return resource;
            }
        }
        public Action<float, float> OnResourceChanged;

        [SerializeField]
        protected float ResourcePerLvl;

        [SerializeField]
        protected float baseResourceReg;
        public float ResourceReg
        {
            protected set;
            get;
        }
        [SerializeField]
        protected float ResourceRegPerLvl;


        [SerializeField]
        protected float baseArmor;
        public float Armor
        {
            protected set;
            get;
        }
        [SerializeField]
        protected float armorPerLvl;


        [SerializeField]
        protected float baseMagicRes;
        public float MagicRes
        {
            protected set;
            get;
        }
        [SerializeField]
        protected float magicResPerLvl;


        [SerializeField]
        protected float baseAtkDmg;
        public float AtkDmg
        {
            protected set;
            get;
        }
        [SerializeField]
        protected float atkDmgPerLvl;


        [SerializeField]
        protected float baseAtkSpeed;
        public float AtkSpeed
        {
            protected set;
            get;
        }
        [SerializeField]
        protected float atkSpeedPerLvl;


        protected float critChance;

        protected float lifesteal;

        public float FlatArmorPen
        {
            protected set;
            get;
        }
        public float PercentArmorPen
        {
            protected set;
            get;
        }


        [SerializeField]
        protected float atkRange;

        public float AtkRange => atkRange;

        public float MagicDmg
        {
            protected set;
            get;
        }

        public float FlatMagicPen
        {
            protected set;
            get;
        }

        public float PercentMagicPen
        {
            protected set;
            get;
        }


        [SerializeField]
        protected float moveSpeed;

        [HideInInspector]
        public Amplifiers amplifiers;


        [SerializeField]
        protected bool canMove = true;

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
        protected bool targetable = true;

        public bool Targetable
        {
            set
            {
                if (targetable != value)
                {
                    if (value)
                    {
                        OnBecomeTargetable?.Invoke();
                    }
                    else
                    {
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



        protected float timeSinceLastRegTick = 0;

        [SerializeField]
        protected Movement movement;

        [SerializeField]
        protected Attacking attacking;


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
            baseHPReg += HPRegPerLvl;
            HPReg += HPRegPerLvl;

            MaxResource += ResourcePerLvl;
            Resource += ResourcePerLvl;
            baseResourceReg += ResourceRegPerLvl;

            baseAtkDmg += atkDmgPerLvl;

            baseArmor += armorPerLvl;
            baseMagicRes += magicResPerLvl;
        }

        public void UpdateStats()
        {
            //items + buffs + base
        }

        protected void ValidateUnitList<T>(List<T> list) where T : Unit
        {
            int n = 0;
            while (n < list.Count)
            {
                if (!list[n])
                {
                    list.RemoveAt(n);
                }
                else n++;
            }
        }

        protected Unit GetClosestUnit<T>(List<T> fromList) where T : Unit
        {
            ValidateUnitList(fromList);
            if (fromList.Count == 0) return null;
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

        public void ReceiveDamage(Unit instigator, float amount, DamageType type)
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

        protected void Die(Unit killer)
        {
            if (killer is Champ)
            {
                var champ = (Champ)killer;
                champ.XP += GetXPReward();
                champ.AddGold(GetGoldReward());
            }
            OnDeath?.Invoke();
        }


        public Action OnDeath;

        public Action<Unit, float, DamageType> OnDealDamage;


        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
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
            Armor = baseArmor;
            MagicRes = baseMagicRes;

            FlatArmorPen = 0;
            PercentArmorPen = 0;

            FlatMagicPen = 0;
            PercentMagicPen = 0;

            timeSinceLastRegTick = 0;

            amplifiers = new Amplifiers();
            amplifiers.Initialize();

            movement?.Initialize(moveSpeed);

            OnLevelUp += (int a) => print(gameObject.name + " reached lvl " + a);

            OnDeath += () => print(gameObject.name + " died.");

            SetupMaterials();
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
                outlineMaterial.shader = ChampHUD.Instance.outline;
                outlineMaterial.SetColor("_OutlineColor", GetOutlineColor());
                outlineMaterial.SetFloat("_Outline", 0.5f);
                outlineMaterials.Add(outlineMaterial);
            }
        }

        protected virtual Color GetOutlineColor()
        {
            if (IsAlly(ChampHUD.Player))
            {
                return ChampHUD.Instance.defaultColors.allyOutline;
            }
            return ChampHUD.Instance.defaultColors.enemyOutline;
        }

        public virtual Color GetHPColor()
        {
            if (IsAlly(ChampHUD.Player))
            {
                return ChampHUD.Instance.defaultColors.allyMinionHP;
            }
            return ChampHUD.Instance.defaultColors.enemyMinionHP;
        }

        private void OnMouseEnter()
        {
            if (!Targetable) return;
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
            if (!Targetable) return;
            HideOutlines();
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


        public virtual float GetXPReward()
        {
            return 10;
        }

        public virtual int GetGoldReward()
        {
            return 25;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
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