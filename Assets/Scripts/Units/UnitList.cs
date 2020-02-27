using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    /// <summary>
    /// Custom list for units that automatically removes any dead / destroyed units, contains functions to find allies / enemies / targetables / closest unit
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UnitList<T> : IEnumerable where T : Unit
    {
        private List<T> list;

        public UnitList()
        {
            list = new List<T>();
        }

        public UnitList(List<T> _list)
        {
            list = new List<T>(_list);
        }

        /// <summary>
        /// Returns a UnitList containing all allies of source of type T in this
        /// </summary>
        /// <param name="source">unit to filter allies for</param>
        /// <returns></returns>
        public UnitList<T> FindAllies(Unit source)
        {
            return new UnitList<T>(list.FindAll(x => source.IsAlly(x) && x is T));
        }

        /// <summary>
        /// Returns a UnitList containing all enemies of source of type T in this
        /// </summary>
        /// <param name="source">unit to tilfer enemies for</param>
        /// <returns></returns>
        public UnitList<T> FindEnemies(Unit source)
        {
            return new UnitList<T>(list.FindAll(x => source.IsEnemy(x) && x is T));
        }


        public T this[int idx] => list[idx];

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        /// <summary>
        /// Validates units before returning the enumerator
        /// </summary>
        /// <returns></returns>
        public UnitEnumerator<T> GetEnumerator()
        {
            Validate();
            return new UnitEnumerator<T>(list);
        }

        public void Add(T unit)
        {
            list.Add(unit);
        }

        public void Remove(T unit)
        {
            list.Remove(unit);
        }

        /// <summary>
        /// Validates units before returning count
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            Validate();
            return list.Count;
        }

        /// <summary>
        /// Removes all dead / destroyed units inside
        /// </summary>
        private void Validate()
        {
            int n = 0;
            while (n < list.Count)
            {
                if (!list[n])
                {
                    list.RemoveAt(n);
                }
                else if (list[n].IsDead)
                {
                    list.RemoveAt(n);
                }
                else n++;
            }
        }

        public bool Contains(T unit)
        {
            return list.Contains(unit);
        }

        /// <summary>
        /// Gets the closest unit of type T1 around source
        /// </summary>
        /// <typeparam name="T1">type of unit to look for</typeparam>
        /// <param name="source">position to calculate distance for</param>
        /// <returns></returns>
        public T1 GetClosestUnitFrom<T1>(Vector3 source) where T1 : T
        {
            if (Count() == 0) return null;
            float lowestDistance = Mathf.Infinity;
            T1 closestUnit = null;
            foreach (T1 unit in this)
            {
                float distance = Vector3.Distance(source, unit.transform.position);
                if (distance < lowestDistance)
                {
                    lowestDistance = distance;
                    closestUnit = unit;
                }
            }
            return closestUnit;
        }

        /// <summary>
        /// Returns a UnitList with all targetable units of type T1 in this
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <returns></returns>
        public UnitList<T1> GetTargetables<T1>() where T1 : T
        {
            UnitList<T1> result = new UnitList<T1>();
            if (Count() == 0) return result;
            foreach (T1 unit in this)
            {
                if (!unit.Targetable) continue;
                result.Add(unit);
            }
            return result;
        }

    }

    /// <summary>
    /// Enumerator for Units
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UnitEnumerator<T> : IEnumerator where T : Unit
    {
        public List<T> units;

        int position = -1;

        public UnitEnumerator(List<T> list)
        {
            units = list;
        }

        public bool MoveNext()
        {
            position++;
            return (position < units.Count);
        }

        public void Reset()
        {
            position = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public Unit Current
        {
            get
            {
                try
                {
                    return units[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }
    }
}
