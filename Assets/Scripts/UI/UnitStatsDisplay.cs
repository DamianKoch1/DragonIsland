using System;
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


        private Action<float> SetADAction;
        private Action<float> SetMDAction;
        private Action<float> SetArmorAction;
        private Action<float> SetMRAction;
        private Action<float> SetAtkSpeedAction;
        private Action<float> SetCDRAction;
        private Action<float> SetCritChanceAction;
        private Action<float> SetMoveSpeedAction;

        private Action<float> SetHPRegAction;
        private Action<float> SetResourceRegAction;
        private Action<int> SetLevelAction;


        public static void UnsubscribeIfInvalidText<T>(ref Action<T> publisher, ref Action<T> subscriber, MaskableGraphic elementToChange)
        {
            if (!elementToChange)
            {
                publisher -= subscriber;
            }
        }

        protected void BindStatTextChangeAction<T>(ref Action<T> publisher, ref Action<T> subscriber, Text textToChange)
        {
            subscriber = (T value) =>
            {
                SetValueText(textToChange, value);
            };
            publisher += subscriber;
        }

        //TODO remove lambdas and subtract functions again on target killed
        public void Initialize(Unit _target)
        {
            Target = _target;

            Target.OnHPChanged += SetHPText;
            SetHPText(Target.HP, Target.MaxHP);

            Target.OnResourceChanged += SetResourceText;
            SetResourceText(Target.Resource, Target.MaxResource);

            SetADAction += (float _) => UnsubscribeIfInvalidText(ref Target.OnADChanged, ref SetADAction, atkDmg);
            BindStatTextChangeAction(ref Target.OnADChanged, ref SetADAction, atkDmg);
            SetValueText(atkDmg, Target.AtkDmg);

            SetMDAction += (float _) => UnsubscribeIfInvalidText(ref Target.OnMagicDmgChanged, ref SetMDAction, mgcDmg);
            BindStatTextChangeAction(ref Target.OnMagicDmgChanged, ref SetMDAction, mgcDmg);
            SetValueText(mgcDmg, Target.MagicDmg);

            SetADAction += (float _) => UnsubscribeIfInvalidText(ref Target.OnAtkSpeedChanged, ref SetAtkSpeedAction, atkSpeed);
            BindStatTextChangeAction(ref Target.OnAtkSpeedChanged, ref SetAtkSpeedAction, atkSpeed);
            SetValueText(atkSpeed, Target.AtkSpeed);

            SetCDRAction += (float _) => UnsubscribeIfInvalidText(ref Target.OnCDRChanged, ref SetCDRAction, cdr);
            BindStatTextChangeAction(ref Target.OnCDRChanged, ref SetCDRAction, cdr);
            SetValueText(cdr, Target.CDReduction);

            SetArmorAction += (float _) => UnsubscribeIfInvalidText(ref Target.OnArmorChanged, ref SetArmorAction, armor);
            BindStatTextChangeAction(ref Target.OnArmorChanged, ref SetArmorAction, armor);
            SetValueText(armor, Target.Armor);

            SetMRAction += (float _) => UnsubscribeIfInvalidText(ref Target.OnMagicResChanged, ref SetMRAction, mgcRes);
            BindStatTextChangeAction(ref Target.OnMagicResChanged, ref SetMRAction, mgcRes);
            SetValueText(mgcRes, Target.MagicRes);

            SetCritChanceAction += (float _) => UnsubscribeIfInvalidText(ref Target.OnCritChanceChanged, ref SetCritChanceAction, critChance);
            BindStatTextChangeAction(ref Target.OnCritChanceChanged, ref SetCritChanceAction, critChance);
            SetValueText(critChance, Target.CritChance);

            SetMoveSpeedAction += (float _) => UnsubscribeIfInvalidText(ref Target.OnMoveSpeedChanged, ref SetMoveSpeedAction, moveSpeed);
            BindStatTextChangeAction(ref Target.OnMoveSpeedChanged, ref SetMoveSpeedAction, moveSpeed);
            SetValueText(moveSpeed, Target.MoveSpeed);

            SetHPRegAction += (float _) => UnsubscribeIfInvalidText(ref Target.OnHPRegChanged, ref SetHPRegAction, hpReg);
            BindStatTextChangeAction(ref Target.OnHPRegChanged, ref SetHPRegAction, hpReg);
            SetValueText(hpReg, Target.HPReg);

            SetResourceRegAction += (float _) => UnsubscribeIfInvalidText(ref Target.OnResourceRegChanged, ref SetResourceRegAction, resourceReg);
            BindStatTextChangeAction(ref Target.OnResourceRegChanged, ref SetResourceRegAction, resourceReg);
            SetValueText(resourceReg, Target.ResourceReg);

            SetLevelAction += (int _) => UnsubscribeIfInvalidText(ref Target.OnLevelUp, ref SetLevelAction, level);
            BindStatTextChangeAction(ref Target.OnLevelUp, ref SetLevelAction, level);
            SetValueText(level, Target.Lvl);

            Target.OnBeforeDeath += OnTargetKilled;
        }

        public void OnTargetKilled()
        {
            Target.OnBeforeDeath -= OnTargetKilled;
            if (Target != PlayerController.Player || !Target)
            {
                Destroy(gameObject);
            }
        }

        private void SetHPText(float current, float max)
        {
            if (!hp)
            {
                Target.OnHPChanged -= SetHPText;
                return;
            }

            hp.text = current + " / " + max;
            if (!hpReg) return;
            if (hpReg.gameObject.activeSelf)
            {
                if (current >= max || Target.HPReg == 0)
                {
                    hpReg.gameObject.SetActive(false);
                }
            }
            else if (current < max && Target.HPReg > 0)
            {
                hpReg.gameObject.SetActive(true);
            }
        }

        private void SetResourceText(float current, float max)
        {
            if (!resource)
            {
                Target.OnResourceChanged -= SetResourceText;
                return;
            }
            resource.text = current + " / " + max;
            if (!resourceReg) return;
            if (resourceReg.gameObject.activeSelf)
            {
                if (current >= max || Target.HPReg == 0)
                {
                    resourceReg.gameObject.SetActive(false);
                }
            }
            else if (current < max && Target.ResourceReg > 0)
            {
                resourceReg.gameObject.SetActive(true);
            }
        }

        private void SetValueText<T>(Text text, T value)
        {
            if (!text) return;
            text.text = value + "";
        }
    }
}