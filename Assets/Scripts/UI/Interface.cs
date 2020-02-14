using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    //TODO stop instantiating enemy displays, initialize with target
    public class Interface : ChampStatBars
    {
        [Space]
        [SerializeField]
        private UnitStatsDisplay playerDisplay;

        [SerializeField]
        private UnitStatsDisplay enemyDisplayPrefab;

        private UnitStatsDisplay enemyDisplayInstance;

        [SerializeField]
        private GameObject skillsDisplay;

        [SerializeField]
        private BarTextTimer castTimeDisplay;

        [SerializeField]
        private Text goldText;

        public override void Initialize(Champ _target)
        {
            base.Initialize(_target);

            playerDisplay?.Initialize(target);
            target.OnGoldChanged += SetGold;

            var skillDisplays = skillsDisplay.GetComponentsInChildren<SkillDisplay>();
            for (int i = 0; i < target.Skills.Count; i++)
            {
                var skill = target.Skills[i];
                skillDisplays[i].Initialize(skill);

                if (skill.CastTime > 0)
                {
                    skill.OnCast += () => castTimeDisplay.Initialize(skill.CastTime);
                    skill.OnRemainingCastTimeChanged += castTimeDisplay.SetRemainingTime;
                    skill.OnCastTimeFinished += () => castTimeDisplay.gameObject.SetActive(false);
                }
            }
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
