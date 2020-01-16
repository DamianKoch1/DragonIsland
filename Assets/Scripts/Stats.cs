using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    [Serializable]
    public class Stats
    {
        
    }

    [Serializable]
    public struct Amplifiers
    {
        public float hp;
        public float heal;

        public float physDmg;
        public float magicDmg;

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
