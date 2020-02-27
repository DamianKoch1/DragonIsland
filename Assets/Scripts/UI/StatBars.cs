using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    /// <summary>
    /// Base class for hp / resource bars, create child classes for unit types
    /// </summary>
    /// <typeparam name="T">type of target unit</typeparam>
    public abstract class StatBars<T> : MonoBehaviour where T : Unit
    {

        protected T target;
        protected UnitStats targetStats;

        [Space]
        [SerializeField]
        protected Image HPBar;

        [SerializeField]
        protected Image HPShadowBar;

        [SerializeField]
        protected Image ResourceBar;

        [Space]
        [SerializeField]
        private Transform HUD;

        [Space]
        [SerializeField]
        private bool animateDamage;

        private Coroutine HPShadowAnim;

        [SerializeField]
        private AnimationCurve HPShadowAnimCurve;

        /// <summary>
        /// Updates hp bar, shows an afterburner if animateDamage is enabled
        /// </summary>
        /// <param name="newAmount"></param>
        /// <param name="max"></param>
        public void SetHP(float newAmount, float max)
        {
            float newFillAmount = newAmount / max;
            if (HPBar.fillAmount == newFillAmount) return;

            if (animateDamage)
            {
                UpdateHPShadow(newFillAmount);
            }

            HPBar.fillAmount = newFillAmount;
        }

        /// <summary>
        /// If hp got reduced, play an afterburn animation from pre-damage to current hp
        /// </summary>
        /// <param name="newHPFillAmount">new fill amount of hp bar</param>
        private void UpdateHPShadow(float newHPFillAmount)
        {
            if (newHPFillAmount >= HPBar.fillAmount) return;
            if (newHPFillAmount > HPShadowBar.fillAmount)
            {
                HPShadowBar.fillAmount = newHPFillAmount;
                return;
            }
            if (HPShadowAnim != null)
            {
                StopCoroutine(HPShadowAnim);
            }
            HPShadowAnim = StartCoroutine(ShowHPShadow(newHPFillAmount));
        }

        /// <summary>
        /// Updates resource bar
        /// </summary>
        /// <param name="newAmount"></param>
        /// <param name="max"></param>
        public void SetResource(float newAmount, float max)
        {
            if (ResourceBar.fillAmount == newAmount / max) return;
            ResourceBar.fillAmount = newAmount / max;
        }

        /// <summary>
        /// Updates hp shadow bar from current to target fill amount using HPShadowAnimCurve over 0.5s
        /// </summary>
        /// <param name="to">target fill amount</param>
        /// <returns></returns>
        private IEnumerator ShowHPShadow(float to)
        {
            float from = HPShadowBar.fillAmount;
            float time = 0;
            while (time < 0.5f)
            {
                HPShadowBar.fillAmount = Mathf.Lerp(from, to, HPShadowAnimCurve.Evaluate(time));
                time += Time.deltaTime;
                yield return null;
            }
            HPShadowBar.fillAmount = HPBar.fillAmount;
        }

        /// <summary>
        /// Initialize for target, initializes WorldPosHUD component if present using offset, sets scale / animation mode
        /// </summary>
        /// <param name="_target"></param>
        /// <param name="_yOffset"></param>
        /// <param name="scale"></param>
        /// <param name="_animateDamage"></param>
        public void Initialize(T _target, float _yOffset = 0, float scale = 1, bool _animateDamage = false)
        {
            animateDamage = _animateDamage;
            HPShadowBar?.gameObject.SetActive(animateDamage);
            Initialize(_target);
            GetComponent<WorldPosHUD>()?.Initialize(_target, _yOffset);
            HUD.localScale *= scale;

        }


        /// <summary>
        /// Saves target, sets bar values
        /// </summary>
        /// <param name="_target"></param>
        public virtual void Initialize(T _target)
        {
            target = _target;
            targetStats = target.Stats;
            SetHP(targetStats.HP, targetStats.MaxHP);

            SetResource(targetStats.Resource, targetStats.MaxResource);

            if (!target.Targetable)
            {
                Toggle(false);
            }

            HPBar.color = target.GetHPColor();
            if (animateDamage)
            {
                HPShadowBar.color = HPBar.color * 0.75f;
                HPShadowBar.fillAmount = HPBar.fillAmount;
            }
        }

        /// <summary>
        /// Update stats if necessary, toggle on / off depending on target being dead / untargetable
        /// </summary>
        private void Update()
        {
            if (!target) Destroy(gameObject);

            SetHP(targetStats.HP, targetStats.MaxHP);

            SetResource(targetStats.Resource, targetStats.MaxResource);

            if (HUD)
            {
                if (target.IsDead)
                {
                    Toggle(false);
                }
                else
                {
                    if (IsVisible)
                    {
                        if (!target.Targetable)
                        {
                            Toggle(false);
                        }
                    }
                    else if (target.Targetable)
                    {
                        Toggle(true);
                    }
                }
            }

        }

        /// <summary>
        /// If object to toggle is assigned, toggles it
        /// </summary>
        /// <param name="show">show (true) or hide (false)?</param>
        protected void Toggle(bool show)
        {
            if (!HUD) return;
            HUD.gameObject.SetActive(show);
        }

        protected bool IsVisible => HUD.gameObject.activeSelf;


        private void OnValidate()
        {
            HPShadowBar?.gameObject.SetActive(animateDamage);
        }
    }
}
