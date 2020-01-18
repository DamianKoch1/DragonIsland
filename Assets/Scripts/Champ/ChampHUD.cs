using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class ChampHUD : StatBarHUD
    {
        private Champ targetChamp;

        [SerializeField]
        private Image XPBar;

        [SerializeField]
        private Text LevelText;

        protected override void Initialize()
        {
            base.Initialize();
            targetChamp = (Champ)target;
            XPBar.fillAmount = 0;
            LevelText.text = "1";
        }
    }
}
