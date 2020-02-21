using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MOBA
{
    public class Tower : Structure
    {
        private UnitList<Unit> enemyUnitsInRange;
        private UnitList<Champ> enemyChampsInRange;

        private LineRenderer lr;

        [SerializeField, Range(0, 1)]
        private float noMinionsDamageMultiplier = 0.5f;

        private RangeIndicator rangeIndicator;

        private void OnTriggerEnter(Collider other)
        {
            if (!PhotonNetwork.IsMasterClient) return;

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

        private void OnTriggerExit(Collider other)
        {
            if (!PhotonNetwork.IsMasterClient) return;

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
                if (unit == attacking.target)
                {
                    CheckForNewTarget();
                }
            }
        }

        protected void OnChampEnteredRange(Champ champ)
        {
            if (this.IsAlly(champ))
            {
                champ.AddNearbyAlliedTower(this);
            }
            else if (!enemyChampsInRange.Contains(champ))
            {
                enemyChampsInRange.Add(champ);
                if (!attacking.IsAttacking())
                {
                    attacking.StartAttacking(champ);
                    rangeIndicator.ToggleOn(stats.AtkRange);
                }
            }
        }

        protected void OnChampExitedRange(Champ champ)
        {
            if (this.IsAlly(champ))
            {
                champ.RemoveNearbyTower(this);
            }
            else if (enemyChampsInRange.Contains(champ))
            {
                enemyChampsInRange.Remove(champ);
                if (champ == attacking.target)
                {
                    CheckForNewTarget();
                }
            }
        }





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

        //TODO: shared code with minion, move to ai targeting component
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
    }
}
