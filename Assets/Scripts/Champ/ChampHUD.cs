using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    //TODO: also serves as current player in network, split up or rename
    public class ChampHUD : StatBarHUD<Champ>
    {
        private static ChampHUD instance;

        public static ChampHUD Instance
        {
            set => instance = value;
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType<ChampHUD>();
                }
                return instance;
            }
        }

        public static Champ Player => Instance.target;

        [SerializeField]
        private Image XPBar;

        [SerializeField]
        private Text LevelText;

        public DefaultColors defaultColors;

        public Shader outline;

        [SerializeField]
        private ParticleSystem clickVfx;

        protected override void Initialize()
        {
            if (instance && instance != this) Destroy(gameObject);
            base.Initialize();
            XPBar.fillAmount = 0;
            LevelText.text = "1";
            target.OnXPChanged += SetXP;
            target.OnLevelUp += SetLvl;
        }

        protected void SetXP(float newAmount, float max)
        {
            XPBar.fillAmount = newAmount / max;
        }

        protected void SetLvl(int newLvl)
        {
            LevelText.text = newLvl + "";
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (target.MoveToCursor(out var targetPos))
                {
                    Instantiate(clickVfx, targetPos + Vector3.up * 0.2f, Quaternion.identity);
                }
            }
            else if (Input.GetMouseButton(1))
            {
                target.MoveToCursor(out var _);
            }
        }

    }
}
