using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class StatBarHUD<T> : MonoBehaviour where T : Unit
    {
        [SerializeField]
        protected T target;

        [SerializeField]
        protected Image HPBar;

        [SerializeField]
        protected Image HPShadowBar;

        [SerializeField]
        protected Image ResourceBar;

        [SerializeField]
        private Transform HUD;


        [SerializeField]
        private bool animateDamage;

        private Coroutine HPShadowAnim;

        [SerializeField]
        private AnimationCurve HPShadowAnimCurve;

        public void SetHP(float newAmount, float max)
        {
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
            ResourceBar.fillAmount = newAmount / max;
        }

        private IEnumerator ShowHPShadow(float from, float to)
        {
            float time = 0;
            while (time < 1)
            {
                HPShadowBar.fillAmount = Mathf.Lerp(from, to, HPShadowAnimCurve.Evaluate(time));
                time += Time.deltaTime;
                yield return null;
                continue;
            }
            HPShadowBar.fillAmount = HPBar.fillAmount;
        }

        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            HPBar.fillAmount = 1;
            HPShadowBar.fillAmount = 1;
            ResourceBar.fillAmount = 1;
            target.OnHPChanged += SetHP;
            target.OnResourceChanged += SetResource;
            target.OnBecomeTargetable += () => Toggle(true);
            target.OnBecomeUntargetable += () => Toggle(false);
            if (!target.Targetable)
            {
                Toggle(false);
            }

            HPBar.color = target.GetHPColor();
            if (animateDamage)
            {
                HPShadowBar.color = HPBar.color * 0.8f;
            }
        }

      

        protected void Toggle(bool show)
        {
            HUD.gameObject.SetActive(show);
        }

        private void LateUpdate()
        {
            Vector3 targetPos = Camera.main.WorldToScreenPoint(target.transform.position);
            targetPos.z = 0;
            HUD.position = targetPos;
        }

        private void OnValidate()
        {
            HPShadowBar?.gameObject.SetActive(animateDamage);
        }
    }
}
