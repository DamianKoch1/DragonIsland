using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    [RequireComponent(typeof(MovementChamp))]
    public class Champ : Unit
    {
        protected int currGold;

        protected List<Tower> nearbyTowers;

        [SerializeField]
        private ChampCamera cam;

        [SerializeField]
        private GameObject clickVfx;

        protected override void Update()
        {
            base.Update();
            if (Input.GetMouseButtonDown(1))
            {
                if (MoveToCursor(out var targetPos))
                {
                    Instantiate(clickVfx, targetPos + Vector3.up * 0.2f, Quaternion.identity);
                }
            }
            else if (Input.GetMouseButton(1))
            {
                MoveToCursor(out var _);
            }
        }

        private bool MoveToCursor(out Vector3 targetPos)
        {
            if (cam.GetCursorToWorldPoint(out var mousePos))
            {
                movement.MoveTo(mousePos);
                targetPos = mousePos;
                return true;
            }
            targetPos = Vector3.zero;
            return false;
        }

        protected override void Initialize()
        {
            base.Initialize();
            OnAttackedByChamp += RequestTowerAssist;
        }

        protected void RequestTowerAssist(Champ attacker)
        {
            foreach (var tower in nearbyTowers)
            {
                tower.TryAttack(attacker);
            }
        }

        public void AddGold(int amount)
        {
            currGold += amount;
        }
    }
}
