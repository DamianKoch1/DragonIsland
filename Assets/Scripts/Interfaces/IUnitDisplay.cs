using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public interface IUnitDisplay<T> where T : Unit
    {
        void Initialize(T _target);

        void OnTargetKilled();
    }
}
