using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOBA
{
    public class Minion : Unit
    {
        private Vector3 targetPosition;

        private LaneID targetLane = LaneID.invalid;

        [SerializeField]
        private float maxChaseDistance = 10;

        [SerializeField]
        private float maxWaypointDistance = 20;


        private List<Unit> enemyUnitsInRange;
        private List<Champ> enemyChampsInRange;




        public LaneID TargetLane
        {
            set
            {
                if (targetLane == LaneID.invalid)
                {
                    targetLane = value;
                }
            }
            get => targetLane;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return;
            var unit = other.GetComponent<Unit>();
            if (!unit) return;
            if (!IsEnemy(unit)) return;
            if (unit is Champ)
            {
                OnEnemyChampEnteredRange((Champ)unit);
            }
            else
            {
                OnEnemyUnitEnteredRange(unit);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.isTrigger) return;
            var unit = other.GetComponent<Unit>();
            if (unit)
            {
                if (IsEnemy(unit))
                {
                    if (unit is Champ)
                    {
                        OnEnemyChampExitedRange((Champ)unit);
                    }
                    else
                    {
                        OnEnemyUnitExitedRange(unit);
                    }
                }
            }
        }

        private void OnEnemyChampEnteredRange(Champ enemy)
        {
            if (!enemyChampsInRange.Contains(enemy))
            {
                enemyChampsInRange.Add(enemy);
            }
        }

        private void OnEnemyChampExitedRange(Champ enemy)
        {
            if (enemyChampsInRange.Contains(enemy))
            {
                enemyChampsInRange.Remove(enemy);
            }
        }

        private void OnEnemyUnitEnteredRange(Unit enemy)
        {
            if (!enemyUnitsInRange.Contains(enemy))
            {
                enemyUnitsInRange.Add(enemy);
            }
        }

        private void OnEnemyUnitExitedRange(Unit enemy)
        {
            if (enemyUnitsInRange.Contains(enemy))
            {
                enemyUnitsInRange.Remove(enemy);
            }
        }

        public void OnReachedWaypoint(LaneWaypoint waypoint)
        {
            switch (TeamID)
            {
                case TeamID.blue:
                    targetPosition = waypoint.GetNextPosition();
                    break;
                case TeamID.red:
                    targetPosition = waypoint.GetPrevPosition();
                    break;
                default:
                    break;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!attacking.IsAttacking())
            {
                movement.MoveTo(targetPosition);
                CheckForNewTarget();
            }
            else if (Vector3.Distance(transform.position, targetPosition) > maxWaypointDistance)
            {
                attacking.Stop();
            }
            else if (Vector3.Distance(transform.position, attacking.CurrentTarget.transform.position) > maxChaseDistance)
            {
                CheckForNewTarget();
            }
        }

        //TODO: shared code with tower, move to ai targeting component
        protected void CheckForNewTarget()
        {
            ValidateUnitList(enemyUnitsInRange);
            if (enemyUnitsInRange.Count > 0)
            {
                var unitTargets = GetTargetables(enemyUnitsInRange);
                if (unitTargets.Count > 0)
                {
                    attacking.StartAttacking(GetClosestUnit(unitTargets));
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

        protected override void Initialize()
        {
            base.Initialize();

            enemyUnitsInRange = new List<Unit>();
            enemyChampsInRange = new List<Champ>();
        }

        public override float GetXPReward()
        {
            return Lvl * 10;
        }

        public override int GetGoldReward()
        {
            return 25;
        }

        protected override void SetupBars()
        {
            Instantiate(statBars).GetComponent<UnitStatBars>()?.Initialize(this, 0.5f, 0.5f);
        }
    }
}
