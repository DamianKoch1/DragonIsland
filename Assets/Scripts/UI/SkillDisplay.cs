using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    /// <summary>
    /// Used to display range / name / rank up button / rank / cost / cooldown / toggle state of a skill
    /// </summary>
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

        [SerializeField]
        private Text rankText;

        private Skill sourceSkill;

        [SerializeField]
        private Button skillPointButton;

        Animation animations;

        /// <summary>
        /// Saves given skill, initializes visuals with skill values, subscribes skill events
        /// </summary>
        /// <param name="skill"></param>
        public void Initialize(Skill skill)
        {
            sourceSkill = skill;
            cdBar.gameObject.SetActive(false);
            cdText.gameObject.SetActive(false);
            button.interactable = true;

            icon.sprite = skill.Icon;

            animations = GetComponent<Animation>();

            skill.OnCDFinished += OnSkillCDFinished;

            button.interactable = false;
            button.onClick.AddListener(skill.OnButtonClicked);

            if (skill is SkillToggleable)
            {
                Initialize((SkillToggleable)skill);
                return;
            }
            costText.text = skill.Cost + "";
            rankText.text = skill.Rank + "";

            skill.OnCast += OnSkillCast;
            skill.OnRemainingCDChanged += OnRemainingCDUpdated;
        }

        /// <summary>
        /// Updates cost / rank texts
        /// </summary>
        private void RefreshTexts()
        {
            if (costText.text != sourceSkill.Cost + "")
            {
                costText.text = sourceSkill.Cost + "";
            }
            if (rankText.text != sourceSkill.Rank + "")
            {
                rankText.text = sourceSkill.Rank + "";
            }
        }

        /// <summary>
        /// Shows / hides skill point button depending if source skill can still be leveled
        /// </summary>
        /// <param name="newAmount"></param>
        public void OnSkillPointsChanged(int newAmount)
        {
            if (sourceSkill.CanBeLeveled(newAmount))
            {
                ShowSkillPointButton();
            }
            else
            {
                HideSkillPointButton();
            }
        }

        /// <summary>
        /// Shows skill point button if not shown already
        /// </summary>
        private void ShowSkillPointButton()
        {
            if (skillPointButton.interactable) return;
            skillPointButton.interactable = true;
            animations.Play("SkillPointAppear");
        }

        /// <summary>
        /// Hides skill point button if not hiding already
        /// </summary>
        private void HideSkillPointButton()
        {
            if (!skillPointButton.interactable) return;
            skillPointButton.interactable = false;
            if (!animations.IsPlaying("SkillPointClicked"))
            {
                animations.Play("SkillPointHide");
            }
        }

        /// <summary>
        /// Decrement owner skill points, hide button if skill can't be leveled anymore, update texts
        /// </summary>
        public void OnSkillPointButtonClicked()
        {
            var owner = (Champ)sourceSkill.Owner;
            if (sourceSkill.Rank == 0)
            {
                button.interactable = true;
            }
            owner.SpendSkillPoint(sourceSkill);
            if (!sourceSkill.CanBeLeveled(owner.AvailableSkillPoints))
            {
                animations.Play("SkillPointClicked");
            }
            RefreshTexts();
        }

        /// <summary>
        /// Saves given toggle skill, initializes visuals with skill values, subscribes toggle skill events
        /// </summary>
        /// <param name="skill"></param>
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

        /// <summary>
        /// Enables cooldown timer
        /// </summary>
        private void OnSkillCast()
        {
            if (!cdBar.gameObject.activeSelf)
            {
                cdBar.gameObject.SetActive(true);
                cdText.gameObject.SetActive(true);
                button.interactable = false;
            }
        }

        /// <summary>
        /// Toggles icon color, shows cost per sec instead
        /// </summary>
        private void OnSkillToggledOn()
        {
            icon.color = Color.magenta;
            costText.text = ((SkillToggleable)sourceSkill).CostPerSec + "";
        }

        /// <summary>
        /// resets icon color, shows toggle cost again
        /// </summary>
        private void OnSkillToggledOff()
        {
            icon.color = Color.white;
            costText.text = sourceSkill.Cost + "";
        }

        /// <summary>
        /// Updates cooldown bar
        /// </summary>
        /// <param name="remainingTime"></param>
        /// <param name="fullCooldown">initial full cooldown value</param>
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

        /// <summary>
        /// Hide cooldown timer, enable button
        /// </summary>
        private void OnSkillCDFinished()
        {
            if (!cdBar.gameObject.activeSelf) return;
            cdBar.gameObject.SetActive(false);
            cdText.gameObject.SetActive(false);
            button.interactable = true;
        }

        /// <summary>
        /// Shows range indicator displaying cast range
        /// </summary>
        public void OnMouseEnter()
        {
            sourceSkill.OnMouseEnter();
        }

        /// <summary>
        /// Hides range indicator
        /// </summary>
        public void OnMouseExit()
        {
            sourceSkill.OnMouseExit();
        }
    }
}
