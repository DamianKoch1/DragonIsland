using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOBA
{
    public class Tower : Structure
    {
        protected List<Minion> enemyMinionsInRange;
        protected List<Champ> enemyChampsInRange;

        [SerializeField]
        protected Transform projectileSpawnpoint;
        protected Unit CurrentTarget
        {
            set
            {
                if (!attacking.target)
                {
                    attacking.StartAttacking(value);
                }
            }
            get => attacking.target;
        }

        [SerializeField]
        private Attacking attacking;

        private void OnTriggerEnter(Collider other)
        {
            var unit = other.GetComponent<Unit>();
            if (!IsEnemy(unit)) return;
            if (unit is Minion)
            {
                OnMinionEnteredRange((Minion)unit);
                return;
            }
            if (unit is Champ)
            {
                OnChampEnteredRange((Champ)unit);
                return;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var unit = other.GetComponent<Unit>();
            if (unit is Minion)
            {
                OnMinionExitedRange((Minion)unit);
                return;
            }
            if (unit is Champ)
            {
                OnChampExitedRange((Champ)unit);
                return;
            }
        }

        protected void OnMinionEnteredRange(Minion minion)
        {
            if (minion.TeamID != TeamID)
            {
                if (!enemyMinionsInRange.Contains(minion))
                {
                    enemyMinionsInRange.Add(minion);
                    if (!CurrentTarget)
                    {
                        CurrentTarget = minion;
                    }
                }
            }
        }

        protected void OnMinionExitedRange(Minion minion)
        {
            if (enemyMinionsInRange.Contains(minion))
            {
                enemyMinionsInRange.Remove(minion);
                if (minion == CurrentTarget)
                {
                    CurrentTarget = null;
                }
            }
        }

        protected void OnChampEnteredRange(Champ champ)
        {
            if (champ.TeamID != TeamID)
            {
                if (!enemyChampsInRange.Contains(champ))
                {
                    enemyChampsInRange.Add(champ);
                    if (!CurrentTarget)
                    {
                        CurrentTarget = champ;
                    }
                }
            }
        }

        protected void OnChampExitedRange(Champ champ)
        {
            if (enemyChampsInRange.Contains(champ))
            {
                enemyChampsInRange.Remove(champ);
                if (champ == CurrentTarget)
                {
                    CurrentTarget = null;
                }
            }
        }

        protected Unit GetClosestUnit(List<Unit> fromList)
        {
            float lowestDistance = Mathf.Infinity;
            Unit closestUnit = null;
            foreach (var unit in fromList)
            {
                float distance = Vector3.Distance(transform.position, unit.transform.position);
                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                    closestUnit = unit;
                }
            }
            return closestUnit;
        }

        protected override void Update()
        {
            base.Update();

            CheckForNewTarget();

            if (CurrentTarget)
            {
                Debug.DrawLine(projectileSpawnpoint.position, CurrentTarget.transform.position, Color.red);
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
            if (CurrentTarget) return;
            if (enemyMinionsInRange.Count > 0)
            {
                CurrentTarget = GetClosestUnit(enemyMinionsInRange.Cast<Unit>().ToList());
            }
            else if (enemyChampsInRange.Count > 0)
            {
                CurrentTarget = GetClosestUnit(enemyChampsInRange.Cast<Unit>().ToList());
            }
        }

        public void TryAttack(Champ target)
        {
            if (!IsEnemy(target)) return;
            if (CurrentTarget?.GetComponent<Champ>()) return;
            if (!enemyChampsInRange.Contains(target)) return;
            CurrentTarget = target;
        }
    }
}
