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
    }
}
