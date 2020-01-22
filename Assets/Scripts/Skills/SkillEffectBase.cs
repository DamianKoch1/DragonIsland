using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace MOBA
{
    //[CreateAssetMenu(fileName = "Skill", menuName = "ScriptableObjects/SkillEffects/Default", order = 2)]
    public abstract class SkillEffectBase : ScriptableObject
    {
        protected Unit owner;

        public void SetOwner(Unit _owner)
        {
            owner = _owner;
        }

        public abstract void Activate();

        public abstract void Tick();

        public abstract void Deactivate();
    }
}
