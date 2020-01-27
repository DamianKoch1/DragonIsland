using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace MOBA
{
    public abstract class SkillEffect : MonoBehaviour
    {
        protected Unit owner;

        protected TeamID ownerTeamID;

        protected int rank;

        protected Unit target;

        protected Vector3 castTargetPos;
        protected Vector3 castTargetDir;

        protected UnitStats ownerStatsAtActivation;

        [SerializeField]
        protected Scalings scaling;

        [SerializeField, Tooltip("If given position AND target (e.g. from projectile hitting a unit), activate on position or on target?")]
        private bool preferApplyToPosition;

        public virtual void Initialize(Unit _owner, int _rank)
        {
            owner = _owner;
            rank = _rank;
            ownerTeamID = owner.TeamID;
        }

        public void SetScaling(Scalings _scaling)
        {
            scaling = _scaling;
        }

        public void Activate(Vector3 targetPos, Unit target, UnitStats ownerStats)
        {
            if (preferApplyToPosition)
            {
                Activate(targetPos, ownerStats);
            }
            else Activate(this.target, ownerStats);
        }

        public virtual void Activate(Vector3 targetPos, UnitStats ownerStats)
        {
            target = null;
            castTargetPos = targetPos;
            castTargetDir = targetPos - owner.GetGroundPos();
            castTargetDir.Normalize();
        }

        public virtual void Activate(Unit _target, UnitStats ownerStats)
        {
            target = _target;
        }

        public abstract void Activate<T>(UnitList<T> targets, UnitStats ownerStats) where T : Unit;

        public abstract void Tick(UnitStats ownerStats);


        public void Deactivate()
        {
            OnDeactivated();
        }

        protected abstract void OnDeactivated();
    }
}
