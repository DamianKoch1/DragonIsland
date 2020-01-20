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
        private GameObject enemyDisplayPrefab;

        private UnitStatsDisplay enemyDisplayInstance;

        public override void Initialize(Champ _target)
        {
            base.Initialize(_target);

            playerDisplay?.Initialize(target);
        }

        public void ShowTargetStats(Unit _target)
        {
            if (_target == PlayerController.Player) return;
            if (enemyDisplayInstance)
            {
                if (enemyDisplayInstance.Target == _target) return;
                Destroy(enemyDisplayInstance.gameObject);
            }
            //TODO cleanup, use extra TargetDisplay script, doesnt clean itself yet after target destroyed
            var instance = Instantiate(enemyDisplayPrefab);
            enemyDisplayInstance = instance.GetComponentInChildren<UnitStatsDisplay>();
            enemyDisplayInstance.Initialize(_target);
            instance.GetComponentInChildren<UnitStatBars>().Initialize(_target);
        }

        public void HideStats()
        {
            if (enemyDisplayInstance)
            {
                //TODO
                Destroy(enemyDisplayInstance.transform.parent.gameObject);
            }
        }
    }
}
