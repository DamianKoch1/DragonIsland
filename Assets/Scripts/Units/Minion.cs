using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace MOBA
{
    //TODO consider only syncing targetpos or lowering sendrate (massive traffic due to minion / player count)
    /// <summary>
    /// Units spawned by bases, run down the lane they spawned in until death
    /// </summary>
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

        /// <summary>
        /// If other is enemy unit, adds it to respective list of enemy champs / units in range
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (!PhotonNetwork.IsMasterClient) return;

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

        /// <summary>
        /// If other is enemy unit, removes it from respective list of enemy champs / units in range
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit(Collider other)
        {
            if (!PhotonNetwork.IsMasterClient) return;

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

        /// <summary>
        /// Adds given champ to enemyChampsInRange
        /// </summary>
        /// <param name="enemy"></param>
        private void OnEnemyChampEnteredRange(Champ enemy)
        {
            if (!enemyChampsInRange.Contains(enemy))
            {
                enemyChampsInRange.Add(enemy);
            }
        }

        /// <summary>
        /// Removes given champ from enemyChampsInRange
        /// </summary>
        /// <param name="enemy"></param>
        private void OnEnemyChampExitedRange(Champ enemy)
        {
            if (enemyChampsInRange.Contains(enemy))
            {
                enemyChampsInRange.Remove(enemy);
            }
        }

        /// <summary>
        /// Adds given unit to enemyUnitsInRange
        /// </summary>
        /// <param name="enemy"></param>
        private void OnEnemyUnitEnteredRange(Unit enemy)
        {
            if (!enemyUnitsInRange.Contains(enemy))
            {
                enemyUnitsInRange.Add(enemy);
            }
        }

        /// <summary>
        /// Removes given unit from enemyUnitsInRange
        /// </summary>
        /// <param name="enemy"></param>
        private void OnEnemyUnitExitedRange(Unit enemy)
        {
            if (enemyUnitsInRange.Contains(enemy))
            {
                enemyUnitsInRange.Remove(enemy);
            }
        }

        /// <summary>
        /// Move to the next / previous waypoint depending on own teamID
        /// </summary>
        /// <param name="waypoint"></param>
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

        /// <summary>
        /// If master client, do moving / attacking logic
        /// </summary>
        protected override void Update()
        {
            if (!PhotonNetwork.IsMasterClient) return;


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
            else if (Vector3.Distance(transform.position, attacking.target.transform.position) > maxChaseDistance)
            {
                CheckForNewTarget();
            }
        }

        //TODO shared code with tower, move to ai targeting component
        /// <summary>
        /// Attacks the closest enemy unit in range, considers non-champs first, stops attacking if no targets
        /// </summary>
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

        public override void Initialize()
        {
            base.Initialize();

            enemyUnitsInRange = new UnitList<Unit>();
            enemyChampsInRange = new UnitList<Champ>();
            if (photonView.IsMine)
            {
                OnReceiveDamage += (Unit attacker, float _, DamageType __) => OnAttacked(attacker);
            }
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

        /// <summary>
        /// If attacked by an enemy, attacks it back if not already attacking
        /// </summary>
        /// <param name="attacker">attacking unit</param>
        protected void OnAttacked(Unit attacker)
        {
            if (!attacker) return;
            if (attacker.IsDead) return;
            if (attacking.IsAttacking()) return;
            if (this.IsAlly(attacker)) return;
            attacking.StartAttacking(attacker);
        }

        /// <summary>
        /// If killed by champ, increase its minion kills
        /// </summary>
        /// <param name="killer"></param>
        protected override void Die(Unit killer)
        {
            if (killer is Champ)
            {
                ((Champ)killer).OnKillMinion();
            }
            base.Die(killer);
        }
    }
}
