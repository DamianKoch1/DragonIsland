using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public enum LaneID
    {
        invalid = -1,
        top = 0,
        mid = 1,
        bot = 2,
    }
    //TODO WIP
    /// <summary>
    /// Used to spawn empowered waves for the enemy in own lane while destroyed
    /// </summary>
    public class Inhibitor : Structure
    {
        private Base alliedBase;

        [Space]

        [SerializeField]
        private LaneID laneID;

        public override int GetGoldReward()
        {
            return 50;
        }

        public override float GetXPReward()
        {
            return stats.Lvl * 50;
        }

        public override void Initialize()
        {
            base.Initialize();
            foreach (var teamBase in FindObjectsOfType<Base>())
            {
                if (this.IsAlly(teamBase))
                {
                    alliedBase = teamBase;
                    break;
                }
            }
        }
    }

}
