using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class UnitStatsDisplay : MonoBehaviour, IUnitDisplay<Unit>
    {
        private Unit target;

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


        public void Initialize(Unit _target)
        {
            target = _target;

            target.OnHPChanged += (float newHP, float maxHP) => SetHP(newHP, maxHP);
            SetHP(target.HP, target.MaxHP);

            target.OnResourceChanged += (float newResource, float maxResource) => SetResource(newResource, maxResource);
            SetResource(target.Resource, target.MaxResource);

            target.OnADChanged += (float newValue) => SetValue(atkDmg, newValue);
            SetValue(atkDmg, target.AtkDmg);

            target.OnMagicDmgChanged += (float newValue) => SetValue(mgcDmg, newValue);
            SetValue(atkDmg, target.AtkDmg);

            target.OnAtkSpeedChanged += (float newValue) => SetValue(atkSpeed, newValue);
            SetValue(atkSpeed, target.AtkSpeed);

            target.OnCDRChanged += (float newValue) => SetValue(cdr, newValue);
            SetValue(cdr, target.CDReduction);

            target.OnArmorChanged += (float newValue) => SetValue(armor, newValue);
            SetValue(armor, target.Armor);

            target.OnMagicResChanged += (float newValue) => SetValue(mgcRes, newValue);
            SetValue(mgcRes, target.MagicRes);

            target.OnCritChanceChanged += (float newValue) => SetValue(critChance, newValue);
            SetValue(critChance, target.CritChance);

            target.OnMoveSpeedChanged += (float newValue) => SetValue(moveSpeed, newValue);
            SetValue(moveSpeed, target.MoveSpeed);

            target.OnHPRegChanged += (float newValue) => SetValue(hpReg, newValue);
            SetValue(hpReg, target.HPReg);

            target.OnResourceRegChanged += (float newValue) => SetValue(resourceReg, newValue);
            SetValue(resourceReg, target.ResourceReg);

            target.OnBeforeDeath += OnTargetKilled;
        }

        public void OnTargetKilled()
        {
            if (target != PlayerController.Player)
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
            else if (current < max)
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
            else if (current < max)
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