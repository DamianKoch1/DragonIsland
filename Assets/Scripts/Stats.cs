using MOBA.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    [Serializable]
    public class UnitStats
    {
        [SerializeField]
        private int maxLvl = 18;

        public int MaxLvl => maxLvl;

        [HideInInspector]
        public int Lvl = -1;

        public Unit Owner
        {
            private set;
            get;
        }

        public const float MAXATKSPEED = 3;
        public const float MAXCDR = 30;



        private float xp = -1;

        public float XP
        {
            set
            {
                if (xp == value) return;
                if (Lvl >= maxLvl) return;
                xp = value.Truncate(1);
                while (xp >= Owner.GetXPNeededForLevel(Lvl + 1))
                {
                    Lvl++;
                    LevelUpStats();
                    OnLevelUp?.Invoke(Lvl);
                }

                //TODO remove this
                if (Lvl >= maxLvl)
                {
                    OnXPChanged?.Invoke(1, 1);
                    return;
                }
                //

                OnXPChanged?.Invoke(xp - Owner.GetXPNeededForLevel(Lvl), Owner.GetXPNeededForLevel(Lvl + 1) - Owner.GetXPNeededForLevel(Lvl));
            }
            get => xp;
        }

        public Action<float, float> OnXPChanged;

        public void LevelUp()
        {
            if (Lvl >= maxLvl) return;
            XP = Owner.GetXPNeededForLevel(Lvl + 1) + 1;
        }

        public Action<int> OnLevelUp;

        private void LevelUpStats()
        {
            GameLogger.Log(Owner, LogActionType.levelUp, Owner.transform.position);

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


        [Space]
        [SerializeField]
        private float baseHP;

        private float maxHP = -1;
        public float MaxHP
        {
            set
            {
                if (maxHP != value)
                {
                    maxHP = value.Truncate(1);
                    OnHPChanged?.Invoke(hp, maxHP);
                }
            }
            get => maxHP;
        }
        private float hp = -1;
        public float HP
        {
            set
            {
                hp = Mathf.Max(0, value).Truncate(1);
                OnHPChanged?.Invoke(hp, maxHP);
            }
            get => hp;
        }
        public Action<float, float> OnHPChanged;


        [SerializeField]
        private float HPPerLvl;

        [SerializeField]
        private float baseHPReg;

        private float hpReg = -1;
        public float HPReg
        {
            set
            {
                if (hpReg != value)
                {
                    hpReg = value.Truncate(1);
                    OnHPRegChanged?.Invoke(hpReg);
                }
            }
            get => hpReg;
        }
        public Action<float> OnHPRegChanged;

        [SerializeField]
        private float HPRegPerLvl;


        [Space]
        [SerializeField]
        private float baseResource;

        private float maxResource = -1;
        public float MaxResource
        {
            set
            {
                if (maxResource != value)
                {
                    maxResource = value.Truncate(1);
                    OnResourceChanged?.Invoke(resource, maxResource);
                }
            }
            get => maxResource;
        }
        private float resource = -1;
        public float Resource
        {
            set
            {
                resource = value.Truncate(1);
                OnResourceChanged?.Invoke(resource, maxResource);
            }
            get => resource;
        }
        public Action<float, float> OnResourceChanged;

        [SerializeField]
        private float resourcePerLvl;

        [SerializeField]
        private float baseResourceReg;

        private float resourceReg = -1;
        public float ResourceReg
        {
            set
            {
                if (resourceReg != value)
                {
                    resourceReg = value.Truncate(1);
                    OnResourceRegChanged?.Invoke(resourceReg);
                }
            }
            get => resourceReg;
        }
        public Action<float> OnResourceRegChanged;

        [SerializeField]
        private float resourceRegPerLvl;



        private float cdr = -1;
        public float CDReduction
        {
            set
            {
                if (cdr != value)
                {
                    cdr = Mathf.Min(MAXCDR, value.Truncate(1));
                    OnCDRChanged?.Invoke(cdr);
                }
            }
            get => cdr;
        }

        public Action<float> OnCDRChanged;


        [Space]
        [SerializeField]
        private float baseArmor;

        private float armor = -1;
        public float Armor
        {
            set
            {
                if (armor != value)
                {
                    armor = value.Truncate(1);
                    OnArmorChanged?.Invoke(armor);
                }
            }
            get => armor;
        }
        public Action<float> OnArmorChanged;

        [SerializeField]
        private float armorPerLvl;


        [Space]
        [SerializeField]
        private float baseMagicRes;

        private float magicRes = -1;
        public float MagicRes
        {
            set
            {
                if (magicRes != value)
                {
                    magicRes = value.Truncate(1);
                    OnMagicResChanged?.Invoke(magicRes);
                }
            }
            get => magicRes;
        }
        public Action<float> OnMagicResChanged;

        [SerializeField]
        private float magicResPerLvl;


        [Space]
        [SerializeField]
        private float baseAtkDmg;

        private float atkDmg = -1;
        public float AtkDmg
        {
            set
            {
                if (atkDmg != value)
                {
                    atkDmg = value.Truncate(1);
                    OnADChanged?.Invoke(atkDmg);
                }
            }
            get => atkDmg;
        }
        public Action<float> OnADChanged;

        [SerializeField]
        private float atkDmgPerLvl;


        [Space]
        [SerializeField]
        private float baseAtkSpeed;

        

        private float atkSpeed = -1;
        public float AtkSpeed
        {
            set
            {
                if (atkSpeed != value)
                {
                    atkSpeed = Mathf.Min(MAXATKSPEED, value.Truncate(1));
                    OnAtkSpeedChanged?.Invoke(atkSpeed);
                }
            }
            get => atkSpeed;
        }
        public Action<float> OnAtkSpeedChanged;

        [SerializeField]
        private float atkSpeedPerLvl;


        private float critChance = -1;
        public float CritChance
        {
            set
            {
                if (critChance != value)
                {
                    critChance = value.Truncate(1);
                    OnCritChanceChanged?.Invoke(critChance);
                }
            }
            get => critChance;
        }
        public Action<float> OnCritChanceChanged;


        [HideInInspector]
        public float lifesteal = -1;

        [HideInInspector]
        public float flatArmorPen = -1;

        [HideInInspector]
        public float percentArmorPen = -1;


        [Space]
        [SerializeField]
        private float atkRange;

        public float AtkRange => atkRange;


        private float magicDmg = -1;
        public float MagicDmg
        {
            set
            {
                if (magicDmg != value)
                {
                    magicDmg = value.Truncate(1);
                    OnMagicDmgChanged?.Invoke(magicDmg);
                }
            }
            get => magicDmg;
        }
        public Action<float> OnMagicDmgChanged;


        [HideInInspector]
        public float flatMagicPen = -1;

        [HideInInspector]
        public float percentMagicPen = -1;


        [SerializeField]
        private float moveSpeed;

        public float MoveSpeed
        {
            protected set
            {
                if (moveSpeed != value)
                {
                    moveSpeed = value.Truncate(1);
                    OnMoveSpeedChanged?.Invoke(moveSpeed);
                }
            }
            get => moveSpeed;
        }
        public Action<float> OnMoveSpeedChanged;

        /// <summary>
        /// Used to save stats at skill cast time incase unit dies while e.g. projectile flies.
        /// </summary>
        public UnitStats(UnitStats ownerStats)
        {
            Lvl = ownerStats.Lvl;
            maxHP = ownerStats.MaxHP;
            hp = ownerStats.HP;
            hpReg = ownerStats.HPReg;
            maxResource = ownerStats.MaxResource;
            resource = ownerStats.Resource;
            resourceReg = ownerStats.ResourceReg;
            cdr = ownerStats.CDReduction;
            armor = ownerStats.Armor;
            magicRes = ownerStats.MagicRes;
            atkDmg = ownerStats.AtkDmg;
            atkSpeed = ownerStats.AtkSpeed;
            critChance = ownerStats.CritChance;
            flatArmorPen = ownerStats.flatArmorPen;
            percentArmorPen = ownerStats.percentArmorPen;
            magicDmg = ownerStats.MagicDmg;
            flatMagicPen = ownerStats.flatMagicPen;
            percentMagicPen = ownerStats.percentMagicPen;
            moveSpeed = ownerStats.MoveSpeed;
        }

        public void Initialize(Unit _owner)
        {
            Owner = _owner;
            Lvl = 1;
            XP = 0;

            MaxHP = baseHP;
            HP = baseHP;
            HPReg = baseHPReg;

            MaxResource = baseResource;
            Resource = baseResource;
            ResourceReg = baseResourceReg;

            AtkDmg = baseAtkDmg;
            AtkSpeed = baseAtkSpeed;
            MagicDmg = 0;

            Armor = baseArmor;
            MagicRes = baseMagicRes;

            flatArmorPen = 0;
            percentArmorPen = 0;

            flatMagicPen = 0;
            percentMagicPen = 0;

            CDReduction = 0;
            CritChance = 0;
            lifesteal = 0;

            OnMoveSpeedChanged?.Invoke(moveSpeed);
        }

        public void ApplyHPReg()
        {
            if (HP >= MaxHP) return;
            HP = Mathf.Min(MaxHP, HP + HPReg);
        }

        public void ApplyResourceReg()
        {
            if (Resource >= MaxResource) return;
            Resource = Mathf.Min(MaxResource, Resource + ResourceReg);
        }


        public void DebugMode()
        {
            AtkSpeed = 5;
            MoveSpeed = 20;
            AtkDmg = 1000;
            MaxHP = 10000;
            HP = 10000;
            MaxResource = 10000;
            Resource = 10000;
            while (Lvl < 18) LevelUp();
        }

    }


    [Serializable]
    public class Stats
    {
        public float hp;
        public float hpReg;

        public float resource;
        public float resourceReg;

        public float atkDmg;
        public float magicDmg;

        [Range(0, UnitStats.MAXATKSPEED)]
        public float atkSpeed;

        [Range(0, UnitStats.MAXCDR)]
        public float cdReduction;

        public float armor;
        public float magicRe;

        public float critChance;
        public float moveSpeed;

        public float atkRange;

        public float lifesteal;

        public float flatArmorPen;
        public float percentArmorPen;

        public float flatMagicPen;
        public float percentMagicPen;
    }



    [Serializable]
    public class Scalings
    {
        [Range(0, 5), Tooltip("Damage per attack damage")]
        public float ad;

        [Range(0, 5), Tooltip("Damage per magic damage")]
        public float md;

        [Range(0, 100), Tooltip("Damage per level")]
        public int level;

        [Range(0, 5), Tooltip("Damage per armor")]
        public float armor;

        [Range(0, 5), Tooltip("Damage per magic resist")]
        public float magicRes;

        [Space]
        [Range(0, 2), Tooltip("Damage per max HP")]
        public float maxHP;

        [Range(0, 2), Tooltip("Damage per current HP")]
        public float currentHP;

        [Range(0, 2), Tooltip("Damage per missing HP")]
        public float missingHP;

        [Space]
        [Range(0, 2), Tooltip("Damage per target max HP")]
        public float targetMaxHP;

        [Range(0, 2), Tooltip("Damage per target current HP")]
        public float targetCurrentHP;

        [Range(0, 2), Tooltip("Damage per target missing HP")]
        public float targetMissingHP;

        public static float operator *(Scalings scaling, Unit owner)
        {
            return scaling * owner.Stats;
        }

        public static float operator *(Scalings scaling, UnitStats ownerStats)
        {
            return scaling.ad * ownerStats.AtkDmg
                + scaling.md * ownerStats.MagicDmg
                + scaling.level * ownerStats.Lvl
                + scaling.armor * ownerStats.Armor
                + scaling.magicRes * ownerStats.MagicRes
                + scaling.maxHP * ownerStats.MaxHP
                + scaling.currentHP * ownerStats.HP
                + scaling.missingHP * (ownerStats.MaxHP - ownerStats.HP);
        }

        public float GetScalingDamageBonusOnTarget(UnitStats ownerStats, Unit target)
        {
            var targetStats = target.Stats;
            return this * ownerStats
                 + targetMaxHP * targetStats.MaxHP
                 + targetCurrentHP * targetStats.HP
                 + targetMissingHP * (targetStats.MaxHP - targetStats.HP);
        }

        public float GetScalingDamageBonusOnTarget(Unit owner, Unit target)
        {
            return GetScalingDamageBonusOnTarget(owner.Stats, target);

        }

    }

    [Serializable]
    public class Amplifiers
    {
        [Range(0, 5)]
        public float hp;

        [Range(0, 5), Tooltip("Amplifies received healing including HP regen")]
        public float heal;

        [Space]
        [Range(0, 5), Tooltip("Amplifies received physical damage")]
        public float physDmg;

        [Range(0, 5), Tooltip("Amplifies received magic damage")]
        public float magicDmg;

        [Space]
        [Range(0, 5)]
        public float resource;

        [Range(0, 10)]
        public float resourceReg;

        [Space]
        [Range(0, 5)]
        public float dealtDmg;

        [Range(0, 5)]
        public float atkDmg;

        [Range(0, 5)]
        public float atkSpeed;

        [Range(0, 5)]
        public float atkRange;

        [Range(0, 5)]
        public float critDamage;

        [Space]
        [Range(0, 5)]
        public float armor;
        [Range(0, 5)]
        public float magicRes;

        [Space]
        [Range(0, 5)]
        public float moveSpeed;

        [Range(0, 1)]
        public float slow;
        [Range(0, 1)]
        public float disables;

        public void Reset()
        {
            hp = 1;
            heal = 1;
            physDmg = 1;
            magicDmg = 1;
            resource = 1;
            resourceReg = 1;
            dealtDmg = 1;
            atkDmg = 1;
            atkSpeed = 1;
            atkRange = 1;
            critDamage = 2;
            armor = 1;
            magicRes = 1;
            moveSpeed = 1;
            slow = 0;
            disables = 0;
        }
    }
}
