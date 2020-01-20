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

        public override void Initialize(Champ _target)
        {
            base.Initialize(_target);

            target.OnXPChanged += SetXP;
            target.OnLevelUp += SetLvl;
        }

        public override void OnTargetKilled()
        {
        }
    }
}
