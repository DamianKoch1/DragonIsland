using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class ChampUI : MonoBehaviour
    {
        [SerializeField]
        private Champ target;

        [SerializeField]
        private Image HPBar;

        [SerializeField]
        private Image HPShadowBar;

        [SerializeField]
        private Image ResourceBar;

        [SerializeField]
        private Image XPBar;

        [SerializeField]
        private Text LevelText;

        [SerializeField]
        private Transform HUD;


        private Coroutine HPShadowAnim;

        [SerializeField]
        private AnimationCurve HPShadowAnimCurve;

        public void SetHP(float newAmount, float maxHP)
        {
            HPBar.fillAmount = newAmount / maxHP;
            if (HPShadowAnim != null)
            {
                StopCoroutine(HPShadowAnim);
            }
            HPShadowAnim = StartCoroutine(ShowHPShadow(HPShadowBar.fillAmount, HPBar.fillAmount));
        }

        private IEnumerator ShowHPShadow(float from, float to)
        {
            float time = 0;
            while (time < 1)
            {
                HPShadowBar.fillAmount = Mathf.Lerp(from, to, HPShadowAnimCurve.Evaluate(time));
                time += Time.deltaTime;
                yield return null;
            }
            HPShadowBar.fillAmount = HPBar.fillAmount;
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            HPBar.fillAmount = 1;
            HPShadowBar.fillAmount = 1;
            ResourceBar.fillAmount = 1;
            XPBar.fillAmount = 0;
            LevelText.text = "1";
            target.OnHPChanged += SetHP;
        }

        private void LateUpdate()
        {
        }
    }
}
