using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOBA
{
    public class Tower : Structure
    {
        private List<Unit> enemyUnitsInRange;
        private List<Champ> enemyChampsInRange;

        private LineRenderer lr;

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
            if (!IsEnemy(unit)) return;
            OnEnemyUnitEnteredRange(unit);
            return;
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
            if (!IsEnemy(unit)) return;
            OnEnemyUnitExitedRange(unit);
            return;
        }

        protected void OnEnemyUnitEnteredRange(Unit unit)
        {
            if (!enemyUnitsInRange.Contains(unit))
            {
                enemyUnitsInRange.Add(unit);
                if (!attacking.IsAttacking())
                {
                    attacking.StartAttacking(unit);
                }
            }
        }

        protected void OnEnemyUnitExitedRange(Unit unit)
        {
            if (enemyUnitsInRange.Contains(unit))
            {
                enemyUnitsInRange.Remove(unit);
                if (unit == attacking.CurrentTarget)
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
            }
            else if (!enemyChampsInRange.Contains(champ))
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
            else if (enemyChampsInRange.Contains(champ))
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
                lr.enabled = false;
                CheckForNewTarget();
            }
            else
            {
                lr.enabled = true;
                lr.SetPosition(1, attacking.CurrentTarget.transform.position);
            }
        }

        protected override void Initialize()
        {
            base.Initialize();

            enemyUnitsInRange = new List<Unit>();
            enemyChampsInRange = new List<Champ>();
            lr = GetComponent<LineRenderer>();
            if (attacking is AttackingRanged)
            {
                lr.SetPosition(0, ((AttackingRanged)attacking).projectileSpawnpoint.position);
            }
            else lr.SetPosition(0, transform.position);
            lr.enabled = false;
        }

        //TODO: shared code with minion, move to ai targeting component
        protected void CheckForNewTarget()
        {
            ValidateUnitList(enemyUnitsInRange);
            if (enemyUnitsInRange.Count > 0)
            {
                var minionTargets = GetTargetables(enemyUnitsInRange);
                if (minionTargets.Count > 0)
                {
                    attacking.StartAttacking(GetClosestUnit(minionTargets));
                    return;
                }
            }
            ValidateUnitList(enemyChampsInRange);
            if (enemyChampsInRange.Count > 0)
            {
                var champTargets = GetTargetables(enemyChampsInRange);
                if (champTargets.Count > 0)
                {
                    attacking.StartAttacking(GetClosestUnit(champTargets));
                    return;
                }
            }
            if (attacking.IsAttacking()) attacking.Stop();
        }

        public void TryAttack(Champ target)
        {
            if (!IsEnemy(target)) return;
            if (attacking.CurrentTarget?.GetComponent<Champ>()) return;
            if (!enemyChampsInRange.Contains(target)) return;
            attacking.StartAttacking(target);
        }

        public override float GetXPReward()
        {
            return Lvl * 100;
        }

        public override int GetGoldReward()
        {
            return 200;
        }
    }
}
