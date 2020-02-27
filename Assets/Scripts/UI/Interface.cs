using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    /// <summary>
    /// Displays player ChampStatBars on HUD (not world to screen position), can display selected unit stats, also displays owner skills
    /// </summary>
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

        /// <summary>
        /// Initializes skill displays / gold text
        /// </summary>
        /// <param name="_target">local player</param>
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
                _target.OnSkillPointsChanged += skillDisplays[i].OnSkillPointsChanged;
                skillDisplays[i].OnSkillPointsChanged(1);

                if (skill.CastTime > 0)
                {
                    skill.OnCast += () => castTimeDisplay.Initialize(skill.CastTime);
                    skill.OnRemainingCastTimeChanged += castTimeDisplay.SetRemainingTime;
                    skill.OnCastTimeFinished += () => castTimeDisplay.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Updates gold text
        /// </summary>
        /// <param name="value">new gold amount</param>
        private void SetGold(float value)
        {
            goldText.text = value + "";
        }

        /// <summary>
        /// Shows target stats on enemyDisplay, creates one if necessary
        /// </summary>
        /// <param name="_target"></param>
        public void ShowTargetStats(Unit _target)
        {
            if (_target == PlayerController.Player) return;
            if (enemyDisplayInstance)
            {
                if (enemyDisplayInstance.Target == _target) return;
            }
            else
            {
                enemyDisplayInstance = Instantiate(enemyDisplayPrefab.gameObject).GetComponent<UnitStatsDisplay>();
            }

            enemyDisplayInstance.Initialize(_target);
            enemyDisplayInstance?.GetComponent<UnitStatBars>().Initialize(_target);
        }

        /// <summary>
        /// Hides enemy stats display
        /// </summary>
        public void HideTargetStats()
        {
            if (enemyDisplayInstance)
            {
                Destroy(enemyDisplayInstance.gameObject);
            }
        }
    }
}
