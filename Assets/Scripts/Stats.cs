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

        public float atkDmgAmplifier;
        public float atkRangeAmplifier;
        public float critDamageAmplifier;

        public float armorAmplifier;
        public float magicResAmplifier;

        public float moveSpeedAmplifier;

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
            atkDmgAmplifier = 1;
            atkRangeAmplifier = 1;
            critDamageAmplifier = 2;
            armorAmplifier = 1;
            magicResAmplifier = 1;
            moveSpeedAmplifier = 1;
            slow = 0;
            disables = 0;
        }
    }
}
