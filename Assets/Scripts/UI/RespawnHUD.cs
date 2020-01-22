using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class RespawnHUD : MonoBehaviour
    {
        [SerializeField]
        private Image timeBar;

        [SerializeField]
        private Text timeText;

        private float startTime;

        public void Initialize(float respawnTime)
        {
            startTime = respawnTime;
            SetRemainingTime(respawnTime);
        }

        public void SetRemainingTime(float time)
        {
            timeBar.fillAmount = time / startTime;
            timeText.text = (int)time + "";
        }
    }
}
