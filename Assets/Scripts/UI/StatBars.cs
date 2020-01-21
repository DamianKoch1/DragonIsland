using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class StatBars<T> : MonoBehaviour, IUnitDisplay<T> where T : Unit
    {
        [SerializeField]
        protected T target;

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
            HPBar.fillAmount = newAmount / max;
            if (!animateDamage) return;
            if (HPShadowAnim != null)
            {
                StopCoroutine(HPShadowAnim);
            }
            HPShadowAnim = StartCoroutine(ShowHPShadow(HPShadowBar.fillAmount, HPBar.fillAmount));
        }


        public void SetResource(float newAmount, float max)
        {
            if (!ResourceBar) return;
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



        //TODO remove lambdas and subtract functions again on target killed
        public virtual void Initialize(T _target)
        {
            target = _target;
            target.OnHPChanged += SetHP;
            SetHP(target.HP, target.MaxHP);
            if (ResourceBar)
            {
                target.OnResourceChanged += SetResource;
                SetResource(target.Resource, target.MaxResource);
            }
            target.OnBecomeTargetable += () => Toggle(true);
            target.OnBecomeUntargetable += () => Toggle(false);
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
            target.OnBeforeDeath += OnTargetKilled;
        }

        public virtual void OnTargetKilled()
        {
            target.OnBeforeDeath -= OnTargetKilled;
            if (!gameObject) return;
            StopAllCoroutines();
            Destroy(gameObject);
        }
      

        protected void Toggle(bool show)
        {
            if (!HUD) return;
            HUD.gameObject.SetActive(show);
        }

     

        private void OnValidate()
        {
            HPShadowBar?.gameObject.SetActive(animateDamage);
        }
    }
}
