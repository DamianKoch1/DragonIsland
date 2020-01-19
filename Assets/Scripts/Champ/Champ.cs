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

       

        protected override void Update()
        {
            base.Update();
           
        }

        public bool MoveToCursor(out Vector3 targetPos)
        {
            if (attacking.IsAttacking())
            {
                attacking.Stop();
            }
            if (cam.GetCursorToWorldPoint(out var mousePos))
            {
                movement.MoveTo(mousePos);
                targetPos = mousePos;
                return true;
            }
            targetPos = Vector3.zero;
            return false;
        }

        public void StartAttacking(Unit target)
        {
            if (!canAttack) return;
            if (!attacking) return;
            if (!IsEnemy(target)) return;
            attacking.StartAttacking(target);
        }

        public bool IsAttacking()
        {
            if (!attacking) return false;
            return attacking.IsAttacking();
        }

        protected override void Initialize()
        {
            base.Initialize();
            OnAttackedByChamp += RequestTowerAssist;
            nearbyAlliedTowers = new List<Tower>();
        }

        protected override void OnDeath()
        {
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
            if (PlayerController.Player == this) return;
            base.ShowOutlines();
        }

        protected override void HideOutlines()
        {
            if (PlayerController.Player == this) return;
            base.HideOutlines();
        }

        protected override Color GetOutlineColor()
        {
            if (PlayerController.Player == this)
            {
                return PlayerController.Instance.defaultColors.ownOutline;
            }
            return base.GetOutlineColor();
        }

        public override Color GetHPColor()
        {
            if (PlayerController.Player == this)
            {
                return PlayerController.Instance.defaultColors.ownHP;
            }
            if (IsAlly(PlayerController.Player))
            {
                return PlayerController.Instance.defaultColors.allyChampHP;
            }
            return PlayerController.Instance.defaultColors.enemyChampHP;
        }

    }
}
