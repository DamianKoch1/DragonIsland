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

            enemyUnitsInRange = new UnitList<Unit>();
            enemyChampsInRange = new UnitList<Champ>();
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
            if (enemyUnitsInRange.Count() > 0)
            {
                var minionTargets = enemyUnitsInRange.GetTargetables<Unit>();
                if (minionTargets.Count() > 0)
                {
                    attacking.StartAttacking(GetClosestUnit(minionTargets));
                    return;
                }
            }
            if (enemyChampsInRange.Count() > 0)
            {
                var champTargets = enemyChampsInRange.GetTargetables<Champ>();
                if (champTargets.Count() > 0)
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
            if (attacking.IsAttacking())
            {
                if (attacking.CurrentTarget?.GetComponent<Champ>()) return;
            }
            if (!enemyChampsInRange.Contains(target)) return;
            attacking.StartAttacking(target);
        }

        public override float GetXPReward()
        {
            return stats.Lvl * 100;
        }

        public override int GetGoldReward()
        {
            return 200;
        }
        public override void ReceiveDamage(Unit instigator, float amount, DamageType type)
        {
            if (type != DamageType.piercing)
            {
                if (enemyUnitsInRange.Count() == 0)
                {
                    amount *= noMinionsDamageMultiplier;
                }
            }
            base.ReceiveDamage(instigator, amount, type);
        }
    }
}
