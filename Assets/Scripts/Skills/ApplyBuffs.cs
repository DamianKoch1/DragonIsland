using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace MOBA
{
    public class ApplyBuffs : SkillEffect
    {
        [SerializeField]
        private List<Buff> buffs;

        public override void Activate()
        {
            throw new NotImplementedException();
        }

        public override void Tick()
        {
            throw new NotImplementedException();
        }

        protected override void OnDeactivated()
        {
            throw new NotImplementedException();
        }
    }
}
