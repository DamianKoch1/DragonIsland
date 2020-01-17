using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class AttackingRanged : Attacking
    {

        [SerializeField]
        protected ProjectileBase projectile;

        public override void Attack(Unit target)
        {
            print("attack");
        }

       
    }
}