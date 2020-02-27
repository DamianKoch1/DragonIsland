using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    /// <summary>
    /// Used to display timers on a bar and text
    /// </summary>
    public class BarTextTimer : MonoBehaviour
    {
        [SerializeField]
        private Image timeBar;

        [SerializeField]
        private Text timeText;

        private float startTime;

        [SerializeField]
        private bool reversed = false;

        /// <summary>
        /// Saves start time
        /// </summary>
        /// <param name="_startTime">max time of timer</param>
        public void Initialize(float _startTime)
        {
            startTime = _startTime;
            SetRemainingTime(_startTime);
        }

        /// <summary>
        /// Sets current time, adjusts bar fill amount / text accordingly
        /// </summary>
        /// <param name="time">new time</param>
        public void SetRemainingTime(float time)
        {
            var value = time / startTime;
            if (reversed) value = 1 - value;
            if (timeBar.fillAmount == value) return;
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            timeBar.fillAmount = value;
            timeText.text = (int)time + "";
        }
    }
}
