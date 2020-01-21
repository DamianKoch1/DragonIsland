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
        private UnitStatsDisplay enemyDisplayPrefab;

        private UnitStatsDisplay enemyDisplayInstance;


        [SerializeField]
        private Text goldText;

        public override void Initialize(Champ _target)
        {
            base.Initialize(_target);

            playerDisplay?.Initialize(target);
            target.OnGoldChanged += SetGold;
        }

        private void SetGold(float value)
        {
            goldText.text = value + "";
        }

        public void ShowTargetStats(Unit _target)
        {
            if (_target == PlayerController.Player) return;
            if (enemyDisplayInstance)
            {
                if (enemyDisplayInstance.Target == _target) return;
                Destroy(enemyDisplayInstance.gameObject);
            }

            enemyDisplayInstance = Instantiate(enemyDisplayPrefab.gameObject).GetComponent<UnitStatsDisplay>();
            enemyDisplayInstance.Initialize(_target);
            enemyDisplayInstance?.GetComponent<UnitStatBars>().Initialize(_target);
        }

        public void HideTargetStats()
        {
            if (enemyDisplayInstance)
            {
                Destroy(enemyDisplayInstance.gameObject);
            }
        }
    }
}
