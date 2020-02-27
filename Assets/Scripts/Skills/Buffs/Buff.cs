using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    [Serializable]
    public class BuffStats
    {
        public Stats statChanges;

        [Space]
        public Amplifiers amplifiers;

    }

    [Serializable]
    public class BuffFlags
    {
        public bool stun;
        public bool root;
        public bool silence;
        public bool disarm;
        public bool undamageable;
        public bool untargetable;
    }


    [Serializable]
    public class BuffProperties
    {
        public bool isHidden = false;

        public string buffBaseName;

        public Sprite icon;

        [HideInInspector]
        public int rank = 1;

        [Space]
        [Range(-1, 300)]
        public float maxDuration = -1;

        public float maxDurationPerRank = 0;

        [HideInInspector]
        public Unit instigator;
    }

    //TODO WIP, implement networking, stackable buffs
    public abstract class Buff : MonoBehaviour
    {
        [SerializeField]
        protected BuffProperties properties;

        public BuffProperties Properties => properties;

        public bool IsHidden => properties.isHidden;

        protected int rank;

        protected float timeSinceLastTick = 0;

        public string BuffName { protected set; get; }

        public float TimeActive { get; private set; }

        public Unit Instigator => properties.instigator;

        protected UnitStats ownerStatsAtApply;

        public virtual void Initialize(BuffProperties _properties, UnitStats ownerStats)
        {
            TimeActive = 0;
            timeSinceLastTick = Unit.TICKINTERVAL;
            OnActivated();
            properties = _properties;
            BuffName = properties.buffBaseName;
            ownerStatsAtApply = ownerStats;
        }

        /// <summary>
        /// Called when this buff is added to a unit
        /// </summary>
        protected abstract void OnActivated();

        protected virtual void Update()
        {
            TimeActive += Time.deltaTime;
            timeSinceLastTick += Time.deltaTime;
            while (timeSinceLastTick >= Unit.TICKINTERVAL)
            {
                OnTick();
                timeSinceLastTick -= Unit.TICKINTERVAL;
            }
            if (properties.maxDuration > 0)
            {
                if (TimeActive >= properties.maxDuration)
                {
                    Destroy(this);
                }
            }
        }

        /// <summary>
        /// Resets timeActive
        /// </summary>
        public void Refresh()
        {
            TimeActive = 0;
        }

        protected abstract void OnTick();

        protected abstract void OnDestroy();
    }
}
