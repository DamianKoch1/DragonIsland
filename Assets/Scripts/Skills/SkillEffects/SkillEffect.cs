using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace MOBA
{
    public abstract class SkillEffect : MonoBehaviour
    {
        protected Unit owner;

        protected int rank;

        [SerializeField]
        protected HitMode hitMode;

        [SerializeField]
        protected Scalings scalings;

        public virtual void Initialize(Unit _owner, int _rank)
        {
            owner = _owner;
            rank = _rank;
        }

        public abstract void Activate(Vector3 targetPos);

        public abstract void Activate(Unit target);

        public abstract void Tick();

        public void Deactivate()
        {
            OnDeactivated();
            Destroy(this);
        }

        protected abstract void OnDeactivated();
    }
}
