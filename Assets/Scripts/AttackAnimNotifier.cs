using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MOBA
{
    /// <summary>
    /// Used to call attack functions in parents of the animator (which are unreachable for animation events)
    /// </summary>
    public class AttackAnimNotifier : MonoBehaviour
    {
        public UnityEvent OnNotify;

        public void Notify()
        {
            OnNotify?.Invoke();
        }
    }
}
