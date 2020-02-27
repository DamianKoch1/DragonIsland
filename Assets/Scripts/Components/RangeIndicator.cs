using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    //TODO stop using 1 indicator for everything
    /// <summary>
    /// Used to display ranges of a unit using scaled sprites
    /// </summary>
    public class RangeIndicator : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer sprite;


        [SerializeField]
        private float alpha = 0.2f;

        /// <summary>
        /// Sets color depending on team relation to player, range and start state
        /// </summary>
        /// <param name="owner">Owning unit</param>
        /// <param name="range">Start range</param>
        /// <param name="toggledOn">Starts toggled on?</param>
        public void Initialize(Unit owner, float range, bool toggledOn = false)
        {
            Color color;
            if (PlayerController.Player.IsAlly(owner))
            {
               color = PlayerController.Instance.defaultColors.allyStructureHP;
            }
            else
            {
                color = PlayerController.Instance.defaultColors.enemyStructureHP;
            }
            color.a = alpha;
            sprite.color = color;

            if (toggledOn)
            {
                ToggleOn(range);
            }
            else
            {
                SetRange(range);
                ToggleOff();
            }
        }

        /// <summary>
        /// Toggles sprite on if newRange is positive
        /// </summary>
        /// <param name="newRange">New range</param>
        public void ToggleOn(float newRange)
        {
            if (newRange < 0) return;
            sprite.enabled = true;
            SetRange(newRange);
        }

        /// <summary>
        /// If range is negative, toggles sprite off, otherwise only if range is equal to previous range
        /// </summary>
        /// <param name="range">Range to toggle off (-1 always toggles off)</param>
        public void ToggleOff(float range = -1)
        {
            if (range > 0)
            {
                if (range != transform.localScale.x) return;
            }
            sprite.enabled = false;
        }

        /// <summary>
        /// Sets local scale of sprite
        /// </summary>
        /// <param name="range">New range</param>
        public void SetRange(float range)
        {
            if (transform.localScale.x == range) return;
            transform.localScale = new Vector3(range, 1, range);
        }
    }
}
