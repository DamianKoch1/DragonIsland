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

        protected override void Update()
        {
            base.Update();
           
        }

        [HideInInspector]
        public bool attackMoving = false;
       
        public void StartAttacking(Unit target)
        {
            attackMoving = false;
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

        public void StopAttacking()
        {
            attacking.Stop();
        }

        protected override void Initialize()
        {
            base.Initialize();
            OnAttackedByChamp += RequestTowerAssist;
            nearbyAlliedTowers = new List<Tower>();
            attackMoving = false;
            movement.OnReachedDestination += () => attackMoving = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return;
            if (!attackMoving) return;
            if (IsAttacking()) return;
            var unit = other.GetComponent<Unit>();
            if (!unit) return;
            if (!IsEnemy(unit)) return;
            StartAttacking(unit);
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

        public override float GetXPReward()
        {
            return Lvl * 50;
        }

        public override int GetGoldReward()
        {
            return 350;
        }

        protected override void SetupBars()
        {
            Instantiate(statBars).GetComponent<ChampStatBars>()?.Initialize(this, 0.5f, 1.2f, true);
        }
    }
}
