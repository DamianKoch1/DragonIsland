﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace MOBA
{
    //TODO WIP
    /// <summary>
    /// Used to add buffs to units
    /// </summary>
    public class ApplyBuff : SkillEffect
    {
        [Space]
        [SerializeField, Tooltip("Buff targets and owner?")]
        private bool alwaysBuffOwner;

        [SerializeField, Tooltip("Buff only owner?")]
        private bool onlyBuffOwner;

        [Space]
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

        [Space]
        [SerializeField]
        private bool addDamageOverTime;

        [SerializeField]
        private float baseDamage;



        [SerializeField]
        private DamageType dmgType;


        private List<Buff> addedBuffs;

        [Space]
        [SerializeField, Tooltip("Format: ClassName,DisplayName,arg1,... (Class must derive from CustomBuff and be in MOBA namespace, no spaces after comma)")]
        private List<string> customBuffs;

        public override void Initialize(Unit _owner, int _rank)
        {
            base.Initialize(_owner, _rank);
            addedBuffs = new List<Buff>();
            properties.instigator = owner;
        }

        /// <summary>
        /// Buff owner if cast on position
        /// </summary>
        /// <param name="targetPos"></param>
        public override void Activate(Vector3 targetPos)
        {
            base.Activate(targetPos);
            Activate(owner);
        }

        /// <summary>
        /// Buffs target and/or owner
        /// </summary>
        /// <param name="target"></param>
        public override void Activate(Unit target)
        {
            base.Activate(target);
            if (onlyBuffOwner)
            {
                AddBuffs(owner);
                return;
            }
            if (alwaysBuffOwner)
            {
                if (target != owner)
                {
                    AddBuffs(owner);
                }
            }
            AddBuffs(target);
        }

        /// <summary>
        /// Increases maxDuration
        /// </summary>
        public override void LevelUp()
        {
            base.LevelUp();
            properties.maxDuration += properties.maxDurationPerRank;
        }

        /// <summary>
        /// Add each buff to target
        /// </summary>
        /// <param name="target"></param>
        private void AddBuffs(Unit target)
        {
            if (!target) return;
            properties.rank = rank;
            if (addStatBuff)
            {
                if (target.HasBuff(properties.buffBaseName + StatBuff.NAMESUFFIX, out var existing))
                {
                    existing.Refresh();
                }
                else
                {
                    var statBuff = target.AddBuff<StatBuff>();
                    statBuff.Initialize(properties, ownerStatsAtActivation, stats);
                    addedBuffs.Add(statBuff);
                }
            }
            if (addDisable)
            {
                if (target.HasBuff(properties.buffBaseName + Disable.NAMESUFFIX, out var existing))
                {
                    existing.Refresh();
                }
                else
                {
                    var disable = target.AddBuff<Disable>();
                    disable.Initialize(properties, ownerStatsAtActivation, disableFlags);
                    addedBuffs.Add(disable);
                }
            }
            if (addDamageOverTime)
            {
                if (target.HasBuff(properties.buffBaseName + DamageOverTime.NAMESUFFIX, out var existing))
                {
                    existing.Refresh();
                }
                else
                {
                    var DOT = target.AddBuff<DamageOverTime>();
                    DOT.Initialize(properties, ownerStatsAtActivation, target);
                    addedBuffs.Add(DOT);
                }
            }
            foreach (var customBuff in customBuffs)
            {
                TryAddCustomBuff(target, customBuff);
            }
        }

        /// <summary>
        /// Try to add customBuff to target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="customBuff">Custom args</param>
        private void TryAddCustomBuff(Unit target, string customBuff)
        {
            var customBuffArgs = new List<string>(customBuff.Split(','));
            var customBuffName = customBuffArgs[0];
            customBuffArgs.RemoveAt(0);
            if (customBuffArgs.Count == 0)
            {
                Debug.LogError(owner.name + " " + gameObject.name + " tried to add custom buff with too few args! Format: ClassName,DisplayName,arg1,...");
                return;
            }
            if (target.HasBuff(properties.buffBaseName + " (" + customBuffArgs[0] + ")", out var existing))
            {
                existing.Refresh();
                return;
            }
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
            var buff = target.AddCustomBuff(buffType);
            buff.Initialize(properties, ownerStatsAtActivation, customBuffArgs);
            addedBuffs.Add(buff);
        }

        public override void Tick()
        {
        }

        /// <summary>
        /// Destroy all added buffs
        /// </summary>
        protected override void OnDeactivated()
        {
            foreach (Buff buff in addedBuffs)
            {
                Destroy(buff);
            }
        }

        /// <summary>
        /// Buff each target
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targets"></param>
        public override void Activate<T>(UnitList<T> targets)
        {
            foreach (var target in targets)
            {
                Activate(target);
            }
            target = null;
        }
    }
}
