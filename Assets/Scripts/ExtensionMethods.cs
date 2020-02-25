using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public static class ExtensionMethods
    {
        public static float Truncate(this float value, int digitsAfterComma)
        {
            double multiplier = Math.Pow(10.0, digitsAfterComma);
            return (float)(Math.Truncate(value * multiplier) / multiplier);
        }

        public static Vector3 GetGroundPos(this Unit unit)
        {
            return unit.transform.position.NullY();
        }

        public static Vector3 NullY(this Vector3 v)
        {
            return new Vector3(v.x, 0, v.z);
        }

        public static UnitList<T> GetTargetableEnemiesInRange<T>(this Unit unit, Vector3 source, float range, bool includeStructures = false) where T : Unit
        {
            UnitList<T> result = new UnitList<T>();
            foreach (var collider in Physics.OverlapSphere(source, range))
            {
                if (collider.isTrigger) continue;
                var enemy = collider.GetComponent<T>();
                if (!enemy) continue;
                if (!unit.IsEnemy(enemy)) continue;
                if (!enemy.Targetable) continue;
                if (!includeStructures)
                {
                    if (enemy is Structure) continue;
                }
                result.Add(enemy);
            }
            return result;
        }

        public static UnitList<T> GetUnitsInRange<T>(this Unit unit, float range, bool includeStructures = false) where T : Unit
        {
            UnitList<T> result = new UnitList<T>();
            foreach (var collider in Physics.OverlapSphere(unit.GetGroundPos(), range))
            {
                if (collider.isTrigger) continue;
                var other = collider.GetComponent<T>();
                if (!other) continue;
                if (other == unit) continue;
                result.Add(other);
            }
            return result;
        }

        public static Unit GetClosestUnit<T>(this Unit unit, UnitList<T> source) where T : Unit
        {
            if (source.Count() == 0) return null;
            float lowestDistance = Mathf.Infinity;
            Unit closestUnit = null;
            foreach (var other in source)
            {
                float distance = Vector3.Distance(unit.GetGroundPos(), other.GetGroundPos());
                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                    closestUnit = other;
                }
            }
            return closestUnit;
        }

        public static bool IsEnemy(this Unit unit, TeamID id)
        {
            if (unit.TeamID == TeamID.blue)
            {
                if (id == TeamID.red)
                {
                    return true;
                }
            }
            if (unit.TeamID == TeamID.red)
            {
                if (id == TeamID.blue)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsEnemy(this Unit unit, Unit other)
        {
            return unit.IsEnemy(other.TeamID);
        }


        public static bool IsAlly(this Unit unit, TeamID id)
        {
            if (unit.TeamID == TeamID.blue)
            {
                if (id == TeamID.blue)
                {
                    return true;
                }
            }
            if (unit.TeamID == TeamID.red)
            {
                if (id == TeamID.red)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsAlly(this Unit unit, Unit other)
        {
            return unit.IsAlly(other.TeamID);
        }

        public static bool HasBuff(this Unit unit, string buffName, out Buff existing)
        {
            foreach (var buff in unit.BuffsSlot.Buffs)
            {
                if (buff.BuffName.Equals(buffName))
                {
                    existing = buff;
                    return true;
                }
            }
            existing = null;
            return false;
        }

        public static int GetViewID(this Unit from)
        {
            if (from)
            {
                if (!from.IsDead)
                {
                    return PhotonView.Get(from).ViewID;
                }
                return -1;
            }
            return -1;
        }

        public static Unit GetUnitByID(this int viewID)
        {
            if (viewID == -1) return null;
            var unit = PhotonView.Find(viewID);
            if (!unit) return null;
            return unit.GetComponent<Unit>();
        }
    }
}
