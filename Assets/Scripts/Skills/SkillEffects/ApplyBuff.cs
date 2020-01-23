using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace MOBA
{
    public class ApplyBuff : SkillEffect
    {
        [SerializeField]
        private BuffProperties properties;

        [Space]
        [SerializeField]
        private bool addStatBuff;

        [SerializeField]
        private BuffStats stats;

        [Space]
        [SerializeField]
        private bool addDisable;

        [SerializeField]
        private BuffFlags disableFlags;

        /// <summary>
        /// Format: ClassName,arg1,arg2,... (Class must derive from CustomBuff and be in MOBA namespace, no spaces after comma)
        /// </summary>
        [Space]
        [SerializeField, Tooltip("Format: ClassName,arg1,arg2,... (Class must derive from CustomBuff and be in MOBA namespace, no spaces after comma)")]
        private List<string> customBuffs;

        public override void Initialize(Unit _owner, int _rank)
        {
            base.Initialize(_owner, _rank);
            properties.instigator = owner;
        }

        public override void Activate(Vector3 targetPos)
        {
        }

        public override void Activate(Unit target)
        {
            if (addStatBuff)
            {
                target.AddBuff<StatBuff>().Initialize(properties, stats);
            }
            if (addDisable)
            {
                target.AddBuff<Disable>().Initialize(properties, disableFlags);
            }
            foreach (var customBuff in customBuffs)
            {
                TryAddBuff(target, customBuff);
            }

        }

        private void TryAddBuff(Unit target, string customBuff)
        {
            var customBuffArgs = new List<string>(customBuff.Split(','));
            var customBuffName = customBuffArgs[0];
            customBuffArgs.RemoveAt(0);
            var buffTypeName = GetType().Namespace + "." + customBuffName;
            var buffType = Type.GetType(buffTypeName);
            if (buffType == null)
            {
                Debug.LogError(owner.name + " " + gameObject.name + " tried to add buff '" + buffTypeName + "', but no such class could be found!");
                return;
            }
            if (!buffType.IsSubclassOf(typeof(CustomBuff)))
            {
                Debug.LogError(owner.name + " " + gameObject.name + " tried to add buff '" + customBuffName + "', which doesn't derive from 'CustomBuff'!");
                return;
            }
            target.AddCustomBuff(buffType).Initialize(properties, customBuffArgs);
        }

        public override void Tick()
        {
        }

        protected override void OnDeactivated()
        {
        }
    }
}
