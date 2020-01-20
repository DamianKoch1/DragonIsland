using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class Interface : ChampStatBars
    {
        [Space]
        [SerializeField]
        private UnitStatsDisplay playerDisplay;

        [SerializeField]
        private UnitStatsDisplay enemyDisplay;

        public override void Initialize(Champ _target)
        {
            base.Initialize(_target);

            playerDisplay?.Initialize(target);
            enemyDisplay?.Initialize(target);
        }
    }
}
