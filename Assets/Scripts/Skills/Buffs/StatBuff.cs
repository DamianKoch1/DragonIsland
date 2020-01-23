using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class StatBuff : Buff
    {
        [SerializeField]
        private BuffStats stats;

        public BuffStats Stats => stats;

        public void Initialize(BuffProperties _properties, BuffStats _stats)
        {
            Initialize(_properties);
            stats = _stats;
        }

        protected override void OnActivated()
        {
        }

        protected override void OnDestroy()
        {
        }

        protected override void OnTick()
        {
        }
    }
}
