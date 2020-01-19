using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{

    public class PlayerController : StatBarHUD<Champ>
    {
        private static PlayerController instance;

        public static PlayerController Instance
        {
            set => instance = value;
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType<PlayerController>();
                }
                return instance;
            }
        }

        [HideInInspector]
        public Unit hovered;

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

        private void DisplayStats(Unit target)
        {

        }

        private void OnleftClick()
        {
            if (!hovered) return;
            DisplayStats(hovered);
        }

        private void OnRightClick()
        {
            if (hovered)
            {
                target.StartAttacking(hovered);
            }
            else if (target.MoveToCursor(out var targetPos))
            {
                Instantiate(clickVfx, targetPos + Vector3.up * 0.2f, Quaternion.identity);
            }
        }

        private void OnRightMBHeld()
        {
            if (hovered)
            {
                target.StartAttacking(hovered);
            }
            else
            {
                target.MoveToCursor(out var _);
            }
        }

        private void AttackHovered()
        {
            target.StartAttacking(hovered);
        }


        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                OnRightClick();
            }
            else if (Input.GetMouseButton(1))
            {
                OnRightMBHeld();
            }
            if (Input.GetMouseButtonDown(0))
            {
                OnleftClick();
            }
        }

    }
}
