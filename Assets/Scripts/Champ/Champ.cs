using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOBA
{
    [RequireComponent(typeof(NavMovement))]
    public class Champ : Unit
    {
        protected int currGold;

        protected List<Tower> nearbyAlliedTowers;

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

        public bool MoveToCursor(out Vector3 targetPos)
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
            nearbyAlliedTowers = new List<Tower>();
        }

        protected void RequestTowerAssist(Champ attacker)
        {
            ValidateUnitList(nearbyAlliedTowers.Cast<Unit>().ToList());
            foreach (var tower in nearbyAlliedTowers)
            {
                tower.TryAttack(attacker);
            }
        }

        public void AddGold(int amount)
        {
            currGold += amount;
        }

        public void AddNearbyAlliedTower(Tower tower)
        {
            if (!nearbyAlliedTowers.Contains(tower))
            {
                nearbyAlliedTowers.Add(tower);
            }
        }

        public void RemoveNearbyTower(Tower tower)
        {
            if (nearbyAlliedTowers.Contains(tower))
            {
                nearbyAlliedTowers.Remove(tower);
            }
        }

        protected override void ShowOutlines()
        {
            if (ChampHUD.Player == this) return;
            base.ShowOutlines();
        }

        protected override void HideOutlines()
        {
            if (ChampHUD.Player == this) return;
            base.HideOutlines();
        }

        protected override Color GetOutlineColor()
        {
            if (ChampHUD.Player == this)
            {
                return ChampHUD.Instance.defaultColors.ownOutline;
            }
            return base.GetOutlineColor();
        }

        public override Color GetHPColor()
        {
            if (ChampHUD.Player == this)
            {
                return ChampHUD.Instance.defaultColors.ownHP;
            }
            if (IsAlly(ChampHUD.Player))
            {
                return ChampHUD.Instance.defaultColors.allyChampHP;
            }
            return ChampHUD.Instance.defaultColors.enemyChampHP;
        }

    }
}
