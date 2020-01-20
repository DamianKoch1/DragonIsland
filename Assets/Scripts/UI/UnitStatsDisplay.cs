using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class UnitStatsDisplay : MonoBehaviour, IUnitDisplay<Unit>
    {
        public Unit Target
        {
            get;
            private set;
        }

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

            Target.OnHPChanged += (float newHP, float maxHP) => SetHP(newHP, maxHP);
            SetHP(Target.HP, Target.MaxHP);

            Target.OnResourceChanged += (float newResource, float maxResource) => SetResource(newResource, maxResource);
            SetResource(Target.Resource, Target.MaxResource);

            Target.OnADChanged += (float newValue) => SetValue(atkDmg, newValue);
            SetValue(atkDmg, Target.AtkDmg);

            Target.OnMagicDmgChanged += (float newValue) => SetValue(mgcDmg, newValue);
            SetValue(atkDmg, Target.AtkDmg);

            Target.OnAtkSpeedChanged += (float newValue) => SetValue(atkSpeed, newValue);
            SetValue(atkSpeed, Target.AtkSpeed);

            Target.OnCDRChanged += (float newValue) => SetValue(cdr, newValue);
            SetValue(cdr, Target.CDReduction);

            Target.OnArmorChanged += (float newValue) => SetValue(armor, newValue);
            SetValue(armor, Target.Armor);

            Target.OnMagicResChanged += (float newValue) => SetValue(mgcRes, newValue);
            SetValue(mgcRes, Target.MagicRes);

            Target.OnCritChanceChanged += (float newValue) => SetValue(critChance, newValue);
            SetValue(critChance, Target.CritChance);

            Target.OnMoveSpeedChanged += (float newValue) => SetValue(moveSpeed, newValue);
            SetValue(moveSpeed, Target.MoveSpeed);

            if (hpReg)
            {
                Target.OnHPRegChanged += (float newValue) => SetValue(hpReg, newValue);
                SetValue(hpReg, Target.HPReg);
            }

            if (resourceReg)
            {
                Target.OnResourceRegChanged += (float newValue) => SetValue(resourceReg, newValue);
                SetValue(resourceReg, Target.ResourceReg);
            }

            if (level)
            {
                Target.OnLevelUp += (int newValue) => SetValue(level, newValue);
                SetValue(level, Target.Lvl);
            }

            Target.OnBeforeDeath += OnTargetKilled;
        }

        public void OnTargetKilled()
        {
            if (Target != PlayerController.Player)
            {
                Destroy(gameObject);
            }
        }

        private void SetHP(float current, float max)
        {
            hp.text = current + " / " + max;
            if (!hpReg) return;
            if (hpReg.gameObject.activeSelf)
            {
                if (current >= max)
                {
                    hpReg.gameObject.SetActive(false);
                }
            }
            else if (current < max && Target.HPReg > 0)
            {
                hpReg.gameObject.SetActive(true);
            }
        }

        private void SetResource(float current, float max)
        {
            resource.text = current + " / " + max;
            if (!resourceReg) return;
            if (resourceReg.gameObject.activeSelf)
            {
                if (current >= max)
                {
                    resourceReg.gameObject.SetActive(false);
                }
            }
            else if (current < max && Target.ResourceReg > 0)
            {
                resourceReg.gameObject.SetActive(true);
            }
        }

        private void SetValue(Text target, float value)
        {
            if (!target) return;
            target.text = value + "";
        }

    }
}