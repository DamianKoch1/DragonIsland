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
            return Lvl * 50;
        }

        protected override void Initialize()
        {
            base.Initialize();
            foreach (var teamBase in FindObjectsOfType<Base>())
            {
                if (IsAlly(teamBase))
                {
                    alliedBase = teamBase;
                    break;
                }
            }
        }
    }

}
