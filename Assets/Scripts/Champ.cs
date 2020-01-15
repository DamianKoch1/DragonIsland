using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    [RequireComponent(typeof(Movement))]
    public class Champ : Unit
    {
        protected int currGold;

        [SerializeField]
        private ChampCamera cam;

        protected override void Update()
        {
            base.Update();

            if (Input.GetMouseButtonDown(1) || Input.GetMouseButton(1))
            {
                if (cam.GetCursorToWorldPoint(out var pos))
                {
                    movement.SetDestination(pos);
                }
            }
        }
    }
}
