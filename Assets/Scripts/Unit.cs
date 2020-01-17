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
        TeamIDCount = 4
    }

    public abstract class Unit : MonoBehaviour
    {

        [SerializeField]
        protected TeamID teamID = TeamID.invalid;

        public TeamID TeamID => teamID;

        [SerializeField]
        protected int maxLvl = 18;
        protected int lvl = 0;

        protected float xp = 0;

        public float XP
        {
            set
            {
                if (lvl >= maxLvl) return;
                xp = value;
                if (xp == 0) return;
                if (xp >= GetXPNeededForLevel(lvl + 1))
                {
                    OnLevelUp?.Invoke(lvl + 1);
                }
            }
            get
            {
                return xp;
            }
        }


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
        protected float magicRes;
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


        public bool canMove = true;

        public bool targetable = true;

        public bool damageable = true;

        public bool canAttack = true;


        protected float timeSinceLastRegTick = 0;

        [SerializeField]
        protected Movement movement;


        protected virtual float GetXPNeededForLevel(int level)
        {
            return (level - 1) * (level - 1) * 100;
        }

        public void LevelUp()
        {
            XP = GetXPNeededForLevel(lvl + 1);
        }

        public Action<int> OnLevelUp;

        protected virtual void LevelUpStats(int level)
        {
            lvl = level;

            MaxHP += HPPerLvl;
            HP += HPPerLvl;
            baseHPReg += HPRegPerLvl;
            HPReg += HPRegPerLvl;

            MaxResource += ResourcePerLvl;
            Resource += ResourcePerLvl;
            baseResourceReg += ResourceRegPerLvl;

            baseAtkDmg += atkDmgPerLvl;

            baseArmor += armorPerLvl;
            magicRes += magicResPerLvl;
        }

        public void UpdateStats()
        {
            //items + buffs + base
        }

        public void ReceiveDamage(Unit instigator, float amount, DamageType type)
        {
            OnReceiveDamage?.Invoke(instigator, amount, type);
            HP -= amount;
            instigator.OnDealDamage.Invoke(this, amount, type);
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
            var champ = killer.GetComponent<Champ>();
            if (champ)
            {
                champ.XP += GetXPReward();
                champ.AddGold(GetGoldReward());
            }
            OnDeath?.Invoke();
            Destroy(gameObject);
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
            lvl = 1;
            XP = 0;

            HP = baseHP;
            MaxHP = baseHP;

            Resource = baseResource;
            MaxResource = baseResource;

            FlatArmorPen = 0;
            PercentArmorPen = 0;

            FlatMagicPen = 0;
            PercentMagicPen = 0;

            timeSinceLastRegTick = 0;

            amplifiers = new Amplifiers();
            amplifiers.Initialize();

            movement?.Initialize(moveSpeed);

            OnReceiveDamage += (Unit a, float b, DamageType type) => print(b + " " + type + " dmg from " + a.gameObject.name);
            OnDealDamage += (Unit a, float b, DamageType type) => print(b + " " + type + " dmg to " + a.gameObject.name);
            OnLevelUp += (int a) => print("reached lvl " + a);
            OnHPChanged += (float a, float b) => print("hp changed to " + a + " / " + b);
            OnResourceChanged += (float a, float b) => print("resource changed to " + a + " / " + b);
        }

        public void MoveTo(Vector3 destination)
        {
            if (!canMove) return;
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

    }
}