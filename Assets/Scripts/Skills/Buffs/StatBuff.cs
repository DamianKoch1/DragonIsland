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

        public const string NAMESUFFIX = " (Stat change)";

        public void Initialize(BuffProperties _properties, UnitStats ownerStats, BuffStats _stats)
        {
            Initialize(_properties, ownerStats);
            BuffName += NAMESUFFIX;
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
