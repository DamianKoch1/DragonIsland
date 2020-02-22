using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace MOBA
{
    public enum EffectTargetingMode
    {
        inherit = 0,
        closestUnit = 1,
        closestAlly = 2,
        closestEnemy = 3,
        closestAllyChamp = 4,
        closestEnemyChamp = 5,
        self = 6
    }

    /// <summary>
    /// Each effect has to implement their own networking and subEffect triggering, this gameObject has a PhotonView for RPCs.
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

        [SerializeField]
        private EffectTargetingMode targetingMode = EffectTargetingMode.inherit;

        public EffectTargetingMode TargetingMode => targetingMode;


        [SerializeField, Tooltip("Doesn't work on instant effects, attach sub effects to child gameobjects, NOT the one with the 'Skill' component! Usually trigger when this effect finished (end of Dash etc)")]
        private List<SkillEffect> subEffects = new List<SkillEffect>();

        protected void ActivateSubEffects(Vector3 targetPos, Unit _target)
        {
            if (subEffects.Count == 0) return;
            if (_target)
            {
                foreach (var subEffect in subEffects)
                {
                    subEffect.Activate(targetPos, _target);
                }
            }
            else
            {
                ActivateSubEffects(targetPos);
            }
        }

        protected void ActivateSubEffects(Vector3 targetPos)
        {
            if (subEffects.Count == 0) return;
            foreach (var subEffect in subEffects)
            {
                subEffect.Activate(targetPos);
            }
        }

        protected void ActivateSubEffects(Unit _target)
        {
            if (subEffects.Count == 0) return;
            foreach (var subEffect in subEffects)
            {
                subEffect.Activate(_target);
            }
        }

        protected void ActivateSubEffects(UnitList<Unit> targets)
        {
            if (subEffects.Count == 0) return;
            foreach (var subEffect in subEffects)
            {
                subEffect.Activate(targets);
            }
        }

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
            if (subEffects.Count == 0) return;
            foreach (var subEffect in subEffects)
            {
                subEffect.Initialize(_owner, _rank);
            }
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

        protected void ValidateTargetPos(Vector3 _targetPos, out Vector3 validated)
        {
            validated = _targetPos;
            var navMovement = owner.GetComponent<NavMovement>();
            if (!navMovement) return;
            validated = navMovement.ClosestNavigablePos(_targetPos);
        }
    }
}
