using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    [RequireComponent(typeof(Collider))]
    public abstract class ProjectileBase : MonoBehaviour
    {
        [SerializeField]
        protected Movement movement;
        protected abstract void OnTriggerEnter(Collider other);

        protected abstract void OnHit(Unit target);
    }
}
