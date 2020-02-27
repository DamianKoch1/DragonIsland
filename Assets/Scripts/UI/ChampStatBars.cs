using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    /// <summary>
    /// Also displays xp / xp for next level, current level
    /// </summary>
    public class ChampStatBars : StatBars<Champ>
    {
        [Space]
        [SerializeField]
        private Image XPBar;

        [SerializeField]
        private Text LevelText;

        /// <summary>
        /// Updates xp bar with given values
        /// </summary>
        /// <param name="newAmount">new xp amount</param>
        /// <param name="max">xp needed for next level</param>
        protected void SetXP(float newAmount, float max)
        {
            if (XPBar.fillAmount == newAmount / max) return;
            XPBar.fillAmount = newAmount / max;
        }

        /// <summary>
        /// Updates level text
        /// </summary>
        /// <param name="newLvl">level reached</param>
        protected void SetLvl(int newLvl)
        {
            if (LevelText.text == newLvl + "") return;
            LevelText.text = newLvl + "";
        }

        /// <summary>
        /// Initializes bars / text, binds toggling on / off to owner death / respawn events
        /// </summary>
        /// <param name="_target">target champ</param>
        public override void Initialize(Champ _target)
        {
            base.Initialize(_target);

            SetXP(0, 1);
            target.Stats.OnXPChanged += SetXP;

            SetLvl(1);
            target.Stats.OnLevelUp += SetLvl;

            target.OnDeathEvent += () => Toggle(false);
            target.OnRespawn += () => Toggle(true);
        }
    }
}
