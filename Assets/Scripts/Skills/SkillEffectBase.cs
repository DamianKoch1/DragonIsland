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

        public void Initialize(Unit _owner, int _rank)
        {
            owner = _owner;
            rank = _rank;
        }

        public abstract void Activate();

        public abstract void Tick();

        public void Deactivate()
        {
            OnDeactivated();
            Destroy(this);
        }

        protected abstract void OnDeactivated();
    }
}
