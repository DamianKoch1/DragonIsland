using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    /// <summary>
    /// Used to display unit stats as texts
    /// </summary>
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


   
        /// <summary>
        /// Saves target, updates texts for it
        /// </summary>
        /// <param name="_target"></param>
        public void Initialize(Unit _target)
        {
            Target = _target;
            targetStats = Target.Stats;

            UpdateTexts();
        }

        /// <summary>
        /// Updates texts, destroys self if target is destroyed / dead
        /// </summary>
        private void Update()
        {
            UpdateTexts();

            if (!Target || Target.IsDead) Destroy(gameObject);
        }

        /// <summary>
        /// Updates each text to match target stat if necessary
        /// </summary>
        private void UpdateTexts()
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
        }

        /// <summary>
        /// Updates hp text if necessary, if not full hp shows hp regen text and updates it, else hides it
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        private void SetHPText(float current, float max)
        {
            if (hp.text == current + " / " + max) return;
            hp.text = current + " / " + max;

            if (hpReg.gameObject.activeSelf)
            {
                if (current >= max || targetStats.HPReg == 0)
                {
                    hpReg.gameObject.SetActive(false);
                }
                else
                {
                    SetValueText(hpReg, targetStats.HPReg);
                }
            }
            else if (current < max && targetStats.HPReg > 0)
            {
                hpReg.gameObject.SetActive(true);
                SetValueText(hpReg, targetStats.HPReg);
            }
        }

        /// <summary>
        /// Updates resource text if necessary, if not full resource shows resource regen text and updates it, else hides it
        /// </summary>
        /// <param name="current"></param>
        /// <param name="max"></param>
        private void SetResourceText(float current, float max)
        {
            if (resource.text == current + " / " + max) return;
            resource.text = current + " / " + max;

            if (resourceReg.gameObject.activeSelf)
            {
                if (current >= max || targetStats.HPReg == 0)
                {
                    resourceReg.gameObject.SetActive(false);
                }
                else
                {
                    SetValueText(resourceReg, targetStats.ResourceReg);
                }
            }
            else if (current < max && targetStats.ResourceReg > 0)
            {
                resourceReg.gameObject.SetActive(true);
                SetValueText(resourceReg, targetStats.ResourceReg);
            }
        }

        /// <summary>
        /// Sets text of the given text to value
        /// </summary>
        /// <param name="text">text to change</param>
        /// <param name="value">new value for text to show</param>
        private void SetValueText(Text text, object value)
        {
            if (!text) return;
            if (text.text == value + "") return;
            text.text = value + "";
        }
    }
}