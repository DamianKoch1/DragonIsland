using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    [Serializable]
    public class BuffProperties
    {
        public bool isHidden = false;

        public string buffName;

        public Sprite icon;

        [Space]
        [Range(-1, 300)]
        public float maxDuration = -1;

        public float maxDurationPerRank = 0;

        [HideInInspector]
        public Unit instigator;
    }

    public abstract class Buff : MonoBehaviour
    {
        [SerializeField]
        protected BuffProperties properties;

        public BuffProperties Properties => properties;

        public bool IsHidden => properties.isHidden;

        protected int rank;

        protected float timeSinceLastTick = 0;

        public float TimeActive { get; private set; }

        public Unit Instigator { get; }

        public virtual void Initialize(BuffProperties _properties)
        {
            TimeActive = 0;
            timeSinceLastTick = Unit.TICKINTERVAL;
            OnActivated();
            properties = _properties;
        }

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

        protected abstract void OnTick();

        protected abstract void OnDestroy();
    }
}
