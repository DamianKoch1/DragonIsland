using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class ChampStatBars : StatBars<Champ>
    {
        [Space]
        [SerializeField]
        private Image XPBar;

        [SerializeField]
        private Text LevelText;

        protected void SetXP(float newAmount, float max)
        {
            XPBar.fillAmount = newAmount / max;
        }

        protected void SetLvl(int newLvl)
        {
            LevelText.text = newLvl + "";
        }

        public override void Initialize(Champ _target, float _yOffset = 0, float scale = 1, bool _animateDamage = false)
        {
            base.Initialize(_target, _yOffset, scale, _animateDamage);

            target.OnXPChanged += SetXP;
            target.OnLevelUp += SetLvl;
        }

        protected override void OnTargetKilled()
        {
        }
    }
}
