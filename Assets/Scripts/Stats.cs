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

        public int Lvl
        {
            get;
            protected set;
        }

        private float xp = -1;

        public float XP
        {
            set
            {
                if (xp == value) return;
                if (Lvl >= maxLvl) return;
                xp = value.Truncate(1);
                while (xp >= GetXPNeededForLevel(Lvl + 1))
                {
                    OnLevelUp?.Invoke(Lvl + 1);
                }
                OnXPChanged?.Invoke(xp - GetXPNeededForLevel(Lvl), GetXPNeededForLevel(Lvl + 1) - GetXPNeededForLevel(Lvl));
            }
            get => xp;
        }


        public Action<float, float> OnXPChanged;

        [Space]

        [SerializeField]
        private float baseHP;

        private float maxHP = -1;
        public float MaxHP
        {
            protected set
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
            protected set
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
            protected set
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
            protected set
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
            protected set
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
            protected set
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

        private float maxCDR = 30;

        private float cdr = -1;
        public float CDReduction
        {
            protected set
            {
                if (cdr != value)
                {
                    cdr = Mathf.Min(maxCDR, value.Truncate(1));
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
            protected set
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
            protected set
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
            protected set
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

        private float maxAtkSpeed = 3;

        private float atkSpeed = -1;
        public float AtkSpeed
        {
            protected set
            {
                if (atkSpeed != value)
                {
                    atkSpeed = Mathf.Min(maxAtkSpeed, value.Truncate(1));
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
            protected set
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


        private float lifesteal;

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

        [Space]

        [SerializeField]
        private float atkRange;

        public float AtkRange => atkRange;

        private float magicDmg = -1;
        public float MagicDmg
        {
            protected set
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
        private float moveSpeed;

        public float MoveSpeed
        {
            protected set
            {
                if (moveSpeed != value)
                {
                    moveSpeed = value.Truncate(1);
                    movement?.SetSpeed(moveSpeed);
                    OnMoveSpeedChanged?.Invoke(moveSpeed);
                }
            }
            get => moveSpeed;
        }
        public Action<float> OnMoveSpeedChanged;

    }

    [Serializable]
    public class ItemStats
    {

    }


    [Serializable]
    public class Amplifiers
    {
        public float hp;
        public float heal;

        //received
        public float physDmg;
        public float magicDmg;
        //

        public float resource;
        public float resourceReg;

        public float dealtDmg;

        public float atkDmg;
        public float atkSpeed;
        public float atkRange;
        public float critDamage;

        public float armor;
        public float magicRes;

        public float moveSpeed;

        [Range(0, 1)]
        public float slow;
        [Range(0, 1)]
        public float disables;

        public void Initialize()
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
