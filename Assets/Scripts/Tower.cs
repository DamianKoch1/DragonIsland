using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOBA
{
    public class Tower : Structure
    {
        public List<Minion> enemyMinionsInRange;
        public List<Champ> enemyChampsInRange;



        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return;
            var unit = other.GetComponent<Unit>();
            if (!unit) return;
            if (unit is Champ)
            {
                OnChampEnteredRange((Champ)unit);
                return;
            }
            if (IsAlly(unit)) return;
            if (unit is Minion)
            {
                OnEnemyMinionEnteredRange((Minion)unit);
                return;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.isTrigger) return;
            var unit = other.GetComponent<Unit>();
            if (!unit) return;
            if (unit is Champ)
            {
                OnChampExitedRange((Champ)unit);
                return;
            }
            if (IsAlly(unit)) return;
            if (unit is Minion)
            {
                OnEnemyMinionExitedRange((Minion)unit);
                return;
            }
        }

        protected void OnEnemyMinionEnteredRange(Minion minion)
        {
            if (!enemyMinionsInRange.Contains(minion))
            {
                enemyMinionsInRange.Add(minion);
                if (!attacking.IsAttacking())
                {
                    attacking.StartAttacking(minion);
                }
            }
        }

        protected void OnEnemyMinionExitedRange(Minion minion)
        {
            if (enemyMinionsInRange.Contains(minion))
            {
                enemyMinionsInRange.Remove(minion);
                if (minion == attacking.CurrentTarget)
                {
                    CheckForNewTarget();
                }
            }
        }

        protected void OnChampEnteredRange(Champ champ)
        {
            if (IsAlly(champ))
            {
                champ.AddNearbyAlliedTower(this);
                return;
            }
            if (!enemyChampsInRange.Contains(champ))
            {
                enemyChampsInRange.Add(champ);
                if (!attacking.IsAttacking())
                {
                    attacking.StartAttacking(champ);
                }
            }
        }

        protected void OnChampExitedRange(Champ champ)
        {
            if (IsAlly(champ))
            {
                champ.RemoveNearbyTower(this);
            }
            if (enemyChampsInRange.Contains(champ))
            {
                enemyChampsInRange.Remove(champ);
                if (champ == attacking.CurrentTarget)
                {
                    CheckForNewTarget();
                }
            }
        }





        protected override void Update()
        {
            base.Update();

            if (!attacking.IsAttacking())
            {
                CheckForNewTarget();
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            attacking.Initialize(this);
            enemyMinionsInRange = new List<Minion>();
            enemyChampsInRange = new List<Champ>();
        }

        protected void CheckForNewTarget()
        {
            if (enemyMinionsInRange.Count > 0)
            {
                attacking.StartAttacking(GetClosestUnit(enemyMinionsInRange));
            }
            else if (enemyChampsInRange.Count > 0)
            {
                attacking.StartAttacking(GetClosestUnit(enemyChampsInRange));
            }
            else if (attacking.IsAttacking()) attacking.Stop();
        }

        public void TryAttack(Champ target)
        {
            if (!IsEnemy(target)) return;
            if (attacking.CurrentTarget?.GetComponent<Champ>()) return;
            if (!enemyChampsInRange.Contains(target)) return;
            attacking.StartAttacking(target);
        }

       
    }
}
