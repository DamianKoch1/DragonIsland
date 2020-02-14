using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MOBA
{
    public class AttackAnimNotifier : MonoBehaviour
    {
        public UnityEvent OnNotify;

        public void Notify()
        {
            OnNotify?.Invoke();
        }
    }
}
