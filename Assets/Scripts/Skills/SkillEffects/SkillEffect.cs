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
    /// Base class for skill effects, each effect has to implement their own networking, subEffect triggering and ranking up.
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public abstract class SkillEffect : MonoBehaviour
    {
        protected Unit owner;

        protected TeamID ownerTeamID;

        protected int rank;

        protected Unit target;

        protected Vector3 castTargetPos;
        protected Vector3 castTargetDir;

        [SerializeField, Tooltip("Effect won't activate if source skill is below this rank")]
        private int minRank = 0;

        public int MinRank => minRank;

        [HideInInspector]
        public UnitStats ownerStatsAtActivation;

        [SerializeField]
        protected Scalings scaling;

        [SerializeField]
        protected Scalings scalingPerRank;

        [SerializeField, Tooltip("If given position AND target (e.g. from projectile hitting a unit), activate on position or on target?")]
        private bool preferApplyToPosition;

        protected PhotonView photonView;

        [SerializeField]
        private EffectTargetingMode targetingMode = EffectTargetingMode.inherit;

        public EffectTargetingMode TargetingMode => targetingMode;


        [SerializeField, Tooltip("Doesn't work on instant effects, attach sub effects (and a PhotonView) to child gameobjects, NOT the one with the 'Skill' component! Usually trigger when this effect finished (end of Dash etc)")]
        private List<SkillEffect> subEffects = new List<SkillEffect>();

        [SerializeField, Tooltip("Is set when this effect activates, for immediate animations use the field in Skill instead")]
        protected string animatorTrigger;

        /// <summary>
        /// Activates subEffects on target / targetPos (effects decide what to use)
        /// </summary>
        /// <param name="targetPos"></param>
        /// <param name="_target"></param>
        protected void ActivateSubEffects(Vector3 targetPos, Unit _target)
        {
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

        /// <summary>
        /// Activates subEffects on targetPos
        /// </summary>
        /// <param name="targetPos"></param>
        protected void ActivateSubEffects(Vector3 targetPos)
        {
            foreach (var subEffect in subEffects)
            {
                subEffect.Activate(targetPos);
            }
        }

        /// <summary>
        /// Activates subffects on single unit
        /// </summary>
        /// <param name="_target"></param>
        protected void ActivateSubEffects(Unit _target)
        {
            foreach (var subEffect in subEffects)
            {
                subEffect.Activate(_target);
            }
        }

        /// <summary>
        /// Activats subEffects on multiple units
        /// </summary>
        /// <param name="targets"></param>
        protected void ActivateSubEffects(UnitList<Unit> targets)
        {
            foreach (var subEffect in subEffects)
            {
                subEffect.Activate(targets);
            }
        }

        /// <summary>
        /// Saves stats at activation (in case e.g. minions can ever cast skills and get destroyed while a projectile flies, also prevents suddenly buying lots of damage)
        /// </summary>
        /// <param name="stats">Stats to save</param>
        public void SetStatsAtActivation(UnitStats stats)
        {
            ownerStatsAtActivation = new UnitStats(stats);
            foreach (var effect in subEffects)
            {
                effect.SetStatsAtActivation(stats);
            }
        }

        /// <summary>
        /// Saves owner and sets rank, initializes all subEffects
        /// </summary>
        /// <param name="_owner"></param>
        /// <param name="_rank"></param>
        public virtual void Initialize(Unit _owner, int _rank)
        {
            owner = _owner;
            rank = _rank;
            ownerTeamID = owner.TeamID;
            photonView = GetComponent<PhotonView>();
            foreach (var subEffect in subEffects)
            {
                subEffect.Initialize(_owner, _rank);
            }
        }

        /// <summary>
        /// Increases rank / scaling, (usually needs child implementation)
        /// </summary>
        public virtual void LevelUp()
        {
            rank++;
            scaling += scalingPerRank;
        }

        public void SetScaling(Scalings _scaling)
        {
            scaling = _scaling;
        }

        /// <summary>
        /// Activate on targetPos or on _target, depending on preferApplyToPosition
        /// </summary>
        /// <param name="targetPos"></param>
        /// <param name="_target"></param>
        public void Activate(Vector3 targetPos, Unit _target)
        {
            if (preferApplyToPosition)
            {
                Activate(targetPos);
            }
            else Activate(_target);
        }

        /// <summary>
        /// Activate on position
        /// </summary>
        /// <param name="targetPos"></param>
        public virtual void Activate(Vector3 targetPos)
        {
            if (!string.IsNullOrEmpty(animatorTrigger))
            {
                owner.Animator?.SetTrigger(animatorTrigger);
            }
            target = null;
            castTargetPos = targetPos;
            castTargetDir = targetPos - owner.GetGroundPos();
            castTargetDir.Normalize();
        }

        /// <summary>
        /// Activate on single target
        /// </summary>
        /// <param name="_target"></param>
        public virtual void Activate(Unit _target)
        {
            if (!string.IsNullOrEmpty(animatorTrigger))
            {
                owner.Animator?.SetTrigger(animatorTrigger);
            }
            target = _target;
        }

        /// <summary>
        /// Activate on multiple targets
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targets"></param>
        public abstract void Activate<T>(UnitList<T> targets) where T : Unit;

        public abstract void Tick();

        /// <summary>
        /// Called when toggle skill is toggled off / channel skill ends
        /// </summary>
        public void Deactivate()
        {
            OnDeactivated();
        }

        /// <summary>
        /// Called when toggle skill is toggled off / channel skill ends
        /// </summary>
        protected abstract void OnDeactivated();

        /// <summary>
        /// Tries to find closest position to _targetPos using owner NavMovement
        /// </summary>
        /// <param name="_targetPos"></param>
        /// <param name="validated"></param>
        /// <returns>Returns false if no NavMovement on owner</returns>
        protected bool ValidateTargetPos(Vector3 _targetPos, out Vector3 validated)
        {
            validated = _targetPos;
            var navMovement = owner.GetComponent<NavMovement>();
            if (!navMovement) return false;
            validated = navMovement.ClosestNavigablePos(_targetPos);
            return true;
        }
    }
}
