using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    public class ScoreBoard : MonoBehaviour
    {
        [SerializeField]
        private Text blueKDAText;

        [SerializeField]
        private Text blueKillsText;

        [SerializeField]
        private Text redKDAText;

        [SerializeField]
        private Text redKillsText;

        [SerializeField]
        private Text blueTowersKilledText;

        [SerializeField]
        private Text redTowersKilledText;

        [SerializeField]
        private GameObject scoreBoard;

        [SerializeField]
        private GameObject scoreBoardBlue;

        [SerializeField]
        private GameObject scoreBoardRed;

        [SerializeField]
        private ScoreBoardDisplay champDisplayPrefab;

        private List<ScoreBoardDisplay> champDisplays;

        private static ScoreBoard instance;
        public static ScoreBoard Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ScoreBoard>();
                    instance.Initialize();
                }
                return instance;
            }
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                scoreBoard.SetActive(true);
                Refresh();
            }
            else if (Input.GetKeyUp(KeyCode.Tab))
            {
                scoreBoard.SetActive(false);
            }
        }

        public void Refresh()
        {
            var blueData = GameState.Instance.blueTeamData;
            var redData = GameState.Instance.redTeamData;
            blueKillsText.text = blueData.kills + "";
            redKillsText.text = redData.kills + "";

            blueKDAText.text = blueData.kills + " / " + blueData.deaths + " / " + blueData.assists;
            blueKDAText.text = blueData.kills + " / " + blueData.deaths + " / " + blueData.assists;

            blueTowersKilledText.text = "Towers: " + blueData.towersKilled;
            redTowersKilledText.text = "Towers: " + redData.towersKilled;

            foreach (var display in champDisplays)
            {
                display.Refresh();
            }
        }

        public void AddDisplay(Champ target)
        {
            if (target.isDummy) return;
            if (target.TeamID == TeamID.blue)
            {
                var display = Instantiate(champDisplayPrefab, scoreBoardBlue.transform).GetComponent<ScoreBoardDisplay>();
                display.SetTargetChamp(target);
                champDisplays.Add(display);
            }
            else if (target.TeamID == TeamID.red)
            {
                var display = Instantiate(champDisplayPrefab, scoreBoardRed.transform).GetComponent<ScoreBoardDisplay>();
                display.SetTargetChamp(target);
                champDisplays.Add(display);
            }
            Refresh();
        }

        public void Initialize()
        {
            champDisplays = new List<ScoreBoardDisplay>();
           
            Refresh();
        }
    }
}
