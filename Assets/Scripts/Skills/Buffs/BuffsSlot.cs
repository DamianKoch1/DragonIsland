using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    /// <summary>
    /// Holds all buffs parent unit currently has
    /// </summary>
    public class BuffsSlot : MonoBehaviour
    {
        public List<Buff> Buffs => new List<Buff>(GetComponents<Buff>());
    }
}
