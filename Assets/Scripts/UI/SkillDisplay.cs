using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class SkillDisplay : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private Image cdBar;

        [SerializeField]
        private Text cdText;

        [SerializeField]
        private Text costText;

        private Skill sourceSkill;


        public void Initialize(Skill skill)
        {
            sourceSkill = skill;
            cdBar.gameObject.SetActive(false);
            cdText.gameObject.SetActive(false);
            button.interactable = true;

            icon.sprite = skill.Icon;

            skill.OnCDFinished += OnSkillCDFinished;

            button.onClick.AddListener(skill.OnButtonClicked);

            if (skill is SkillToggleable)
            {
                Initialize((SkillToggleable)skill);
                return;
            }
            costText.text = skill.Cost + "";

            skill.OnCast += OnSkillCast;
            skill.OnRemainingCDChanged += OnRemainingCDUpdated;
        }

        private void Initialize(SkillToggleable skill)
        {
            sourceSkill = skill;
            skill.OnCast += OnSkillToggledOn;
            skill.OnToggledOff += OnSkillToggledOff;
            skill.OnRemainingCDChanged += (float remainingTime, float fullCooldown) =>
            {
                if (button.interactable)
                {
                    if (!skill.IsToggledOn)
                    {
                        button.interactable = false;
                    }
                }
                OnRemainingCDUpdated(remainingTime, fullCooldown);
            };
        }

        private void OnSkillCast()
        {
            if (!cdBar.gameObject.activeSelf)
            {
                cdBar.gameObject.SetActive(true);
                cdText.gameObject.SetActive(true);
                button.interactable = false;
            }
        }

        private void OnSkillToggledOn()
        {
            icon.color = Color.magenta;
            costText.text = ((SkillToggleable)sourceSkill).CostPerSec + "";
        }

        private void OnSkillToggledOff()
        {
            icon.color = Color.white;
            costText.text = sourceSkill.Cost + "";
        }

        private void OnRemainingCDUpdated(float remainingTime, float fullCooldown)
        {
            if (!cdBar.gameObject.activeSelf)
            {
                cdBar.gameObject.SetActive(true);
                cdText.gameObject.SetActive(true);
            }
            cdBar.fillAmount = remainingTime / fullCooldown;
            cdText.text = remainingTime.Truncate(0) + "";
        }

        private void OnSkillCDFinished()
        {
            if (!cdBar.gameObject.activeSelf) return;
            cdBar.gameObject.SetActive(false);
            cdText.gameObject.SetActive(false);
            button.interactable = true;
        }
    }
}
