using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOBA
{
    public class TeamData
    {
        public int kills;
        public int assists;
        public int deaths;

        public int towersKilled;

        public TeamData(IEnumerable<Champ> champs)
        {
            kills = 0;
            assists = 0;
            deaths = 0;
            towersKilled = 0;
            foreach (var champ in champs)
            {
                kills += champ.Kills;
                assists += champ.Assists;
                deaths += champ.Deaths;
                towersKilled += champ.TowersKilled;
            }
        }
    }

    public class GameState
    {
        TeamData blueTeamData;

        IEnumerable<Champ> blueChamps;

        TeamData redTeamData;

        IEnumerable<Champ> redChamps;

        private static GameState instance;
        public static GameState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameState();
                }
                return instance;
            }
        }

        public GameState()
        {
            Champ[] champs = Object.FindObjectsOfType<Champ>();
            blueChamps = champs.Where(c => c.TeamID == TeamID.blue);
            redChamps = champs.Where(c => c.TeamID == TeamID.red);
        }
    }
}
