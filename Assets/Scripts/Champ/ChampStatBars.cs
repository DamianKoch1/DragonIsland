using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class ChampStatBars : StatBars<Champ>
    {
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

        public override void Initialize(Champ _target, bool _animateDamage = false)
        {
            base.Initialize(_target, _animateDamage);

            target.OnXPChanged += SetXP;
            target.OnLevelUp += SetLvl;
        }

        protected override void OnTargetKilled()
        {
        }
    }
}
