using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class Disable : Buff
    {
        [SerializeField]
        private BuffFlags flags;

        public BuffFlags Flags => flags;

        public const string NAMESUFFIX = " (Disable)";

        public void Initialize(BuffProperties _properties, UnitStats ownerStats, BuffFlags _flags)
        {
            Initialize(_properties, ownerStats);
            BuffName += NAMESUFFIX;
            flags = _flags;
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
