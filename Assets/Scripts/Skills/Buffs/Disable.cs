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

        public void Initialize(BuffProperties _properties, BuffFlags _flags)
        {
            Initialize(_properties);
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
