using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class StatBars<T> : MonoBehaviour where T : Unit
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

        public void SetHP(float newAmount, float max)
        {
            if (!HPBar) return;
            float newFillAmount = newAmount / max;
            if (HPBar.fillAmount == newFillAmount) return;
            HPBar.fillAmount = newFillAmount;
            if (!animateDamage) return;
            if (HPShadowBar.fillAmount >= newFillAmount) return;
            if (HPShadowAnim != null)
            {
                StopCoroutine(HPShadowAnim);
            }
            HPShadowAnim = StartCoroutine(ShowHPShadow(HPShadowBar.fillAmount, HPBar.fillAmount));
        }


        public void SetResource(float newAmount, float max)
        {
            if (!ResourceBar) return;
            if (ResourceBar.fillAmount == newAmount / max) return;
            ResourceBar.fillAmount = newAmount / max;
        }

        private IEnumerator ShowHPShadow(float from, float to)
        {
            float time = 0;
            while (time < 0.5f)
            {
                HPShadowBar.fillAmount = Mathf.Lerp(from, to, HPShadowAnimCurve.Evaluate(time));
                time += Time.deltaTime;
                yield return null;
                continue;
            }
            HPShadowBar.fillAmount = HPBar.fillAmount;
        }

        public void Initialize(T _target, float _yOffset = 0, float scale = 1, bool _animateDamage = false)
        {
            animateDamage = _animateDamage;
            HPShadowBar?.gameObject.SetActive(animateDamage);
            Initialize(_target);
            GetComponent<WorldPosHUD>()?.Initialize(_target, _yOffset);
            HUD.localScale *= scale;

        }



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

        private void Update()
        {
            SetHP(targetStats.HP, targetStats.MaxHP);

            SetResource(targetStats.Resource, targetStats.MaxResource);

            if (HUD && !target.IsDead)
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

            if (!target) Destroy(gameObject);
        }



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
