using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace MOBA
{
    /// <summary>
    /// Each effect has to implement their own networking, this gameObject has a PhotonView for RPCs.
    /// </summary>
    public abstract class SkillEffect : MonoBehaviour
    {
        protected Unit owner;

        protected TeamID ownerTeamID;

        protected int rank;

        protected Unit target;

        protected Vector3 castTargetPos;
        protected Vector3 castTargetDir;

        [HideInInspector]
        public UnitStats ownerStatsAtActivation;

        [SerializeField]
        protected Scalings scaling;

        [SerializeField, Tooltip("If given position AND target (e.g. from projectile hitting a unit), activate on position or on target?")]
        private bool preferApplyToPosition;

        protected PhotonView photonView;


        public void SetStatsAtActivation(UnitStats stats)
        {
            ownerStatsAtActivation = new UnitStats(stats);
        }

        public virtual void Initialize(Unit _owner, int _rank)
        {
            owner = _owner;
            rank = _rank;
            ownerTeamID = owner.TeamID;
            photonView = GetComponent<PhotonView>();
        }

        public void SetScaling(Scalings _scaling)
        {
            scaling = _scaling;
        }

        public void Activate(Vector3 targetPos, Unit _target)
        {
            if (preferApplyToPosition)
            {
                Activate(targetPos);
            }
            else Activate(_target);
        }

        public virtual void Activate(Vector3 targetPos)
        {
            target = null;
            castTargetPos = targetPos;
            castTargetDir = targetPos - owner.GetGroundPos();
            castTargetDir.Normalize();
        }

        public virtual void Activate(Unit _target)
        {
            target = _target;
        }

        public abstract void Activate<T>(UnitList<T> targets) where T : Unit;

        public abstract void Tick();


        public void Deactivate()
        {
            OnDeactivated();
        }

        protected abstract void OnDeactivated();
    }
}
