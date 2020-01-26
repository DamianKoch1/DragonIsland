using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class BarTextTimer : MonoBehaviour
    {
        [SerializeField]
        private Image timeBar;

        [SerializeField]
        private Text timeText;

        private float startTime;

        [SerializeField]
        private bool reversed = false;

        public void Initialize(float _startTime)
        {
            startTime = _startTime;
            SetRemainingTime(_startTime);
        }

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
