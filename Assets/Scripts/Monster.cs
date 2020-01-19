using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{

    public class Monster : Unit
    {
        protected override Color GetOutlineColor()
        {
            return Color.grey;
        }

        public override Color GetHPColor()
        {
            return Color.magenta;
        }

        public override float GetXPReward()
        {
            return Lvl * 100;
        }

        public override int GetGoldReward()
        {
            return 120;
        }
    }
}
