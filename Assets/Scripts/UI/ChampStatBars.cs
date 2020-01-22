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
            if (XPBar.fillAmount == newAmount / max) return;
            XPBar.fillAmount = newAmount / max;
        }

        protected void SetLvl(int newLvl)
        {
            if (LevelText.text == newLvl + "") return;
            LevelText.text = newLvl + "";
        }

        public override void Initialize(Champ _target)
        {
            base.Initialize(_target);

            target.Stats.OnXPChanged += SetXP;
            target.Stats.OnLevelUp += SetLvl;

            target.OnBeforeDeath += () => Toggle(false);
            target.OnRespawn += () => Toggle(true);
        }
    }
}
