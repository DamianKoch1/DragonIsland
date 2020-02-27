using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    /// <summary>
    /// Used to display team / champ stats like KDA
    /// </summary>
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

        /// <summary>
        /// Toggle on / off if tab pressed / released
        /// </summary>
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

        /// <summary>
        /// Recalculates all stats and updates respective texts
        /// </summary>
        public void Refresh()
        {
            var blueKills = 0;
            var redKills = 0;

            var blueDeaths = 0;
            var redDeaths = 0;

            var blueAssists = 0;
            var redAssists = 0;

            var blueTowersKilled = 0;
            var redTowersKilled = 0;

            foreach (var display in champDisplays)
            {
                display.Refresh();
                var target = display.Target;
                if (target.TeamID == TeamID.blue)
                {
                    blueKills += target.Kills;
                    blueDeaths += target.Deaths;
                    blueAssists += target.Assists;
                    blueTowersKilled += target.TowersKilled;
                }
                else
                {
                    redKills += target.Kills;
                    redDeaths += target.Deaths;
                    redAssists += target.Assists;
                    redTowersKilled += target.TowersKilled;
                }
            }

            blueKillsText.text = blueKills + "";
            redKillsText.text = redKills + "";

            blueKDAText.text = blueKills + " / " + blueDeaths + " / " + blueAssists;
            redKDAText.text = redKills + " / " + redDeaths + " / " + redAssists;

            blueTowersKilledText.text = "Towers: " + blueTowersKilled;
            redTowersKilledText.text = "Towers: " + redTowersKilled;
        }

        /// <summary>
        /// Spawns a scoreboard display for given champ
        /// </summary>
        /// <param name="target"></param>
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
