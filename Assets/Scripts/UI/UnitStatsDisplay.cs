using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class UnitStatsDisplay : MonoBehaviour
    {
        public Unit Target
        {
            get;
            private set;
        }

        private UnitStats targetStats;

        [Space]
        [SerializeField]
        private Text hp;

        [SerializeField]
        private Text resource;

        [SerializeField]
        private Text atkDmg;

        [SerializeField]
        private Text mgcDmg;

        [SerializeField]
        private Text atkSpeed;

        [SerializeField]
        private Text cdr;

        [SerializeField]
        private Text armor;

        [SerializeField]
        private Text mgcRes;

        [SerializeField]
        private Text critChance;

        [SerializeField]
        private Text moveSpeed;

        [SerializeField]
        private Text hpReg;

        [SerializeField]
        private Text resourceReg;

        [SerializeField]
        private Text level;


   

        public void Initialize(Unit _target)
        {
            Target = _target;
            targetStats = Target.Stats;

            SetHPText(targetStats.HP, targetStats.MaxHP);
            SetResourceText(targetStats.Resource, targetStats.MaxResource);

            SetValueText(atkDmg, targetStats.AtkDmg);
            SetValueText(mgcDmg, targetStats.MagicDmg);
            SetValueText(atkSpeed, targetStats.AtkSpeed);
            SetValueText(cdr, targetStats.CDReduction);
            SetValueText(armor, targetStats.Armor);
            SetValueText(mgcRes, targetStats.MagicRes);
            SetValueText(critChance, targetStats.CritChance);
            SetValueText(moveSpeed, targetStats.MoveSpeed);
            SetValueText(level, targetStats.Lvl);
        }

        private void Update()
        {
            SetHPText(targetStats.HP, targetStats.MaxHP);
            SetResourceText(targetStats.Resource, targetStats.MaxResource);

            SetValueText(atkDmg, targetStats.AtkDmg);
            SetValueText(mgcDmg, targetStats.MagicDmg);
            SetValueText(atkSpeed, targetStats.AtkSpeed);
            SetValueText(cdr, targetStats.CDReduction);
            SetValueText(armor, targetStats.Armor);
            SetValueText(mgcRes, targetStats.MagicRes);
            SetValueText(critChance, targetStats.CritChance);
            SetValueText(moveSpeed, targetStats.MoveSpeed);
            SetValueText(level, targetStats.Lvl);

            if (!Target) Destroy(gameObject);
        }

        private void SetHPText(float current, float max)
        {
            if (!hp) return;
            hp.text = current + " / " + max;
            if (!hpReg) return;
            if (hpReg.gameObject.activeSelf)
            {
                if (current >= max || targetStats.HPReg == 0)
                {
                    hpReg.gameObject.SetActive(false);
                }
            }
            else if (current < max && targetStats.HPReg > 0)
            {
                hpReg.gameObject.SetActive(true);
            }
        }

        private void SetResourceText(float current, float max)
        {
            if (!resource) return;
            resource.text = current + " / " + max;
            if (!resourceReg) return;
            if (resourceReg.gameObject.activeSelf)
            {
                if (current >= max || targetStats.HPReg == 0)
                {
                    resourceReg.gameObject.SetActive(false);
                }
            }
            else if (current < max && targetStats.ResourceReg > 0)
            {
                resourceReg.gameObject.SetActive(true);
            }
        }

        private void SetValueText(Text text, object value)
        {
            if (!text) return;
            text.text = value + "";
        }
    }
}