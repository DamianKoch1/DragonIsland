using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Rounds this float to digitsAfterComma
        /// </summary>
        /// <param name="value">float to round</param>
        /// <param name="digitsAfterComma">number digits after comma of resulting float</param>
        /// <returns></returns>
        public static float Truncate(this float value, int digitsAfterComma)
        {
            double multiplier = Math.Pow(10.0, digitsAfterComma);
            return (float)(Math.Truncate(value * multiplier) / multiplier);
        }

        /// <summary>
        /// Returns own position with y set to 0
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static Vector3 GetGroundPos(this Unit unit)
        {
            return unit.transform.position.NullY();
        }

        /// <summary>
        /// Returns the same vector with y set to 0
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector3 NullY(this Vector3 v)
        {
            return new Vector3(v.x, 0, v.z);
        }

        /// <summary>
        /// Returns a UnitList with all targetable enemy units of this of type T within range around source
        /// </summary>
        /// <typeparam name="T">Type of unit to look for</typeparam>
        /// <param name="unit">Unit to filter enemies for</param>
        /// <param name="source">Source position</param>
        /// <param name="range">Max range around source to look for units inside</param>
        /// <param name="includeStructures">Include structures?</param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns a UnitList with all units of type T within range around this
        /// </summary>
        /// <typeparam name="T">Type of unit to look for</typeparam>
        /// <param name="unit">Unit whose position to use as search center</param>
        /// <param name="range">Max range around unit to look for units inside</param>
        /// <param name="includeStructures">Include structures?</param>
        /// <returns></returns>
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

        /// <summary>
        /// Resutns the closest unit of type T from a UnitList to this
        /// </summary>
        /// <typeparam name="T">Type of unit to look for</typeparam>
        /// <param name="unit">Unit to get the closest one in list for</param>
        /// <param name="source">Source UnitList to get the closest one to unit of</param>
        /// <returns></returns>
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

        /// <summary>
        /// Is a unit with given TeamID an enemy of this?
        /// </summary>
        /// <param name="unit">unit to check relation to</param>
        /// <param name="id">TeamID of other unit</param>
        /// <returns>Returns true if own TeamID is blue / red and id is the respective other one, false otherwise</returns>
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

        /// <summary>
        /// Is the given unit an enemy of this?
        /// </summary>
        /// <param name="unit">unit to check relation to</param>
        /// <param name="other">other unit</param>
        /// <returns>Returns true if own TeamID is blue / red and other TeamID is the respective other one, false otherwise</returns>
        public static bool IsEnemy(this Unit unit, Unit other)
        {
            return unit.IsEnemy(other.TeamID);
        }

        /// <summary>
        /// Is a unit with given TeamID an ally of this?
        /// </summary>
        /// <param name="unit">unit to check relation to</param>
        /// <param name="id">TeamID of other unit</param>
        /// <returns>Returns true if own TeamID and id are both blue / red, false otherwise</returns>
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

        /// <summary>
        /// Is a unit with given TeamID an ally of this?
        /// </summary>
        /// <param name="unit">unit to check relation to</param>
        /// <param name="other">other unit</param>
        /// <returns>Returns true if own TeamID and other TeamID are both blue / red, false otherwise</returns>
        public static bool IsAlly(this Unit unit, Unit other)
        {
            return unit.IsAlly(other.TeamID);
        }

        /// <summary>
        /// Checks whether this unit has a buff with given buffName
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="buffName">buffName to search for</param>
        /// <param name="existing">existing instance of the searched buff (null if not found)</param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the ViewID of the PhotonView of this unit
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Finds the unit whose PhotonView has this id
        /// </summary>
        /// <param name="viewID"></param>
        /// <returns></returns>
        public static Unit GetUnitByID(this int viewID)
        {
            if (viewID == -1) return null;
            var unit = PhotonView.Find(viewID);
            if (!unit) return null;
            return unit.GetComponent<Unit>();
        }
    }
}
