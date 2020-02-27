using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOBA
{
    /// <summary>
    /// Structure that attacks enemy units nearby
    /// </summary>
    public class Tower : Structure
    {
        private UnitList<Unit> enemyUnitsInRange;
        private UnitList<Champ> enemyChampsInRange;

        private LineRenderer lr;

        [SerializeField, Range(0, 1)]
        private float noMinionsDamageMultiplier = 0.5f;

        private RangeIndicator rangeIndicator;

        /// <summary>
        /// If other is enemy unit, adds it to respective list of enemy champs / units in range
        /// </summary>
        /// <param name="other"></param>
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
            if (!this.IsEnemy(unit)) return;
            OnEnemyUnitEnteredRange(unit);
            return;
        }

        /// <summary>
        /// If other is enemy unit, removes it from respective list of enemy champs / units in range
        /// </summary>
        /// <param name="other"></param>
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
            if (!this.IsEnemy(unit)) return;
            OnEnemyUnitExitedRange(unit);
            return;
        }

        /// <summary>
        /// Adds given unit to enemyUnitsInRange, attacks it if no previous target
        /// </summary>
        /// <param name="unit"></param>
        protected void OnEnemyUnitEnteredRange(Unit unit)
        {
            if (!enemyUnitsInRange.Contains(unit))
            {
                enemyUnitsInRange.Add(unit);
                if (!PhotonNetwork.IsMasterClient) return;
                if (!attacking.IsAttacking())
                {
                    attacking.StartAttacking(unit);
                }
            }
        }

        /// <summary>
        /// Removes given unit from enemyUnitsInRange, if it was attacking target, check for new one
        /// </summary>
        /// <param name="unit"></param>
        protected void OnEnemyUnitExitedRange(Unit unit)
        {
            if (enemyUnitsInRange.Contains(unit))
            {
                enemyUnitsInRange.Remove(unit);
                if (!PhotonNetwork.IsMasterClient) return;
                if (unit == attacking.target)
                {
                    CheckForNewTarget();
                }
            }
        }

        /// <summary>
        /// If given champ is ally, adds this to its nearby towers, else adds it to enemyChampsInRange, attacks it if no previous targets
        /// </summary>
        /// <param name="champ"></param>
        protected void OnChampEnteredRange(Champ champ)
        {
            if (this.IsAlly(champ))
            {
                if (!PhotonNetwork.IsMasterClient) return;
                champ.AddNearbyAlliedTower(this);
            }
            else if (!enemyChampsInRange.Contains(champ))
            {
                enemyChampsInRange.Add(champ);
                if (!PhotonNetwork.IsMasterClient) return;
                if (!attacking.IsAttacking())
                {
                    attacking.StartAttacking(champ);
                    rangeIndicator.ToggleOn(stats.AtkRange);
                }
            }
        }

        /// <summary>
        /// If given champ is ally, remove this from its nearby towers, else removes it from enemyChampsInRange, if it was attacking target, check for new one
        /// </summary>
        /// <param name="enemy"></param>
        protected void OnChampExitedRange(Champ champ)
        {
            if (this.IsAlly(champ))
            {
                if (!PhotonNetwork.IsMasterClient) return;
                champ.RemoveNearbyAlliedTower(this);
            }
            else if (enemyChampsInRange.Contains(champ))
            {
                enemyChampsInRange.Remove(champ);
                if (!PhotonNetwork.IsMasterClient) return;
                if (champ == attacking.target)
                {
                    CheckForNewTarget();
                }
            }
        }




        /// <summary>
        /// If master client, do attacking logic, render line to attack target
        /// </summary>
        protected override void Update()
        {
            if (!PhotonNetwork.IsMasterClient) return;

            base.Update();

            if (!attacking.IsAttacking())
            {
                lr.enabled = false;
                CheckForNewTarget();
            }
            else
            {
                lr.enabled = true;
                lr.SetPosition(1, attacking.target.transform.position);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            enemyUnitsInRange = new UnitList<Unit>();
            enemyChampsInRange = new UnitList<Champ>();
            lr = GetComponent<LineRenderer>();
            if (attacking is AttackingRanged)
            {
                lr.SetPosition(0, ((AttackingRanged)attacking).projectileSpawnpoint.position);
            }
            else lr.SetPosition(0, transform.position);
            lr.enabled = false;

            rangeIndicator = GetComponentInChildren<RangeIndicator>();
            rangeIndicator.Initialize(this, stats.AtkRange);
        }

        //TODO shared code with minion, move to ai targeting component
        /// <summary>
        /// Attacks the closest enemy unit in range, considers non-champs first, stops attacking if no targets
        /// </summary>
        protected void CheckForNewTarget()
        {
            if (enemyUnitsInRange.Count() > 0)
            {
                var minionTargets = enemyUnitsInRange.GetTargetables<Unit>();
                if (minionTargets.Count() > 0)
                {
                    attacking.StartAttacking(this.GetClosestUnit(minionTargets));
                    rangeIndicator.ToggleOff();
                    return;
                }
            }
            if (enemyChampsInRange.Count() > 0)
            {
                var champTargets = enemyChampsInRange.GetTargetables<Champ>();
                if (champTargets.Count() > 0)
                {
                    attacking.StartAttacking(this.GetClosestUnit(champTargets));
                    rangeIndicator.ToggleOn(stats.AtkRange);
                    return;
                }
            }
            if (attacking.IsAttacking())
            {
                attacking.Stop();
                rangeIndicator.ToggleOff();
            }
        }

        /// <summary>
        /// If given champ is an enemy, attack it unless not already attacking any champ
        /// </summary>
        /// <param name="target"></param>
        public void TryAttack(Champ target)
        {
            if (!this.IsEnemy(target)) return;
            if (attacking.IsAttacking())
            {
                if (attacking.target?.GetComponent<Champ>()) return;
            }
            if (!enemyChampsInRange.Contains(target)) return;
            attacking.StartAttacking(target);
            rangeIndicator.ToggleOn(stats.AtkRange);
        }

        public override float GetXPReward()
        {
            return stats.Lvl * 100;
        }

        public override int GetGoldReward()
        {
            return 200;
        }

        //TODO move reduced damage calculation to masterclient, prevents discrepancies
        /// <summary>
        /// Reduces non-piercing damage taken if no enemy minions are nearby
        /// </summary>
        /// <param name="instigatorViewID"></param>
        /// <param name="amount"></param>
        /// <param name="damageType"></param>
        [PunRPC]
        public override void ReceiveDamage(int instigatorViewID, int amount, short damageType)
        {
            var instigator = instigatorViewID.GetUnitByID();
            var type = (DamageType)damageType;
            if (type != DamageType.piercing)
            {
                if (enemyUnitsInRange.Count() == 0)
                {
                    amount = (int)(amount * noMinionsDamageMultiplier);
                }
            }
            base.ReceiveDamage(instigatorViewID, amount, damageType);
        }

        /// <summary>
        /// If killer is champ, increase its towers killed
        /// </summary>
        /// <param name="killer"></param>
        protected override void Die(Unit killer)
        {
            if (killer is Champ)
            {
                ((Champ)killer).OnKillTower();
            }
            base.Die(killer);
        }
    }
}
