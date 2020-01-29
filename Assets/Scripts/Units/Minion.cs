using Photon.Pun;
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


        private UnitList<Unit> enemyUnitsInRange;
        private UnitList<Champ> enemyChampsInRange;




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
            if (!this.IsEnemy(unit)) return;
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
                if (this.IsEnemy(unit))
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
            if (enemyUnitsInRange.Count() > 0)
            {
                var unitTargets = enemyUnitsInRange.GetTargetables<Unit>();
                if (unitTargets.Count() > 0)
                {
                    attacking.StartAttacking(this.GetClosestUnit(unitTargets));
                    return;
                }
            }
            if (enemyChampsInRange.Count() > 0)
            {
                var champTargets = enemyChampsInRange.GetTargetables<Champ>();
                if (champTargets.Count() > 0)
                {
                    attacking.StartAttacking(this.GetClosestUnit(champTargets));
                    return;
                }
            }
            if (attacking.IsAttacking()) attacking.Stop();
        }

        protected override void Initialize()
        {
            base.Initialize();

            enemyUnitsInRange = new UnitList<Unit>();
            enemyChampsInRange = new UnitList<Champ>();
            OnReceiveDamage += (Unit attacker, float _, DamageType __) => OnAttacked(attacker);
        }

        public override float GetXPReward()
        {
            return stats.Lvl * 10;
        }

        public override int GetGoldReward()
        {
            return 25;
        }

        protected override void SetupBars()
        {
            var statBarsGO = Resources.Load<GameObject>("StatBars");
            statBarsInstance = Instantiate(statBarsGO, transform.parent);
            statBarsInstance.GetComponent<UnitStatBars>()?.Initialize(this, 0.5f, 0.3f);
        }

        protected void OnAttacked(Unit attacker)
        {
            if (attacking.IsAttacking()) return;
            if (this.IsAlly(attacker)) return;
            attacking.StartAttacking(attacker);
        }

        protected override void OnDeath()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
