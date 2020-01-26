using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class BuffsSlot : MonoBehaviour
    {
        public List<Buff> Buffs => new List<Buff>(GetComponents<Buff>());
    }
}
