using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{


    /// <summary>
    /// Base class for movement behaviour
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class Movement : MonoBehaviour
    {
        public float lastVelocity
        {
            get;
            protected set;
        }

        protected float speed;

        public Vector3 TargetPos { protected set; get; }

        /// <summary>
        /// Moves to destination (needs implementation by child classes)
        /// </summary>
        /// <param name="destination">Target position</param>
        public virtual void MoveTo(Vector3 destination)
        {
            TargetPos = destination;
        }

        /// <summary>
        /// Stops movement
        /// </summary>
        public abstract void Stop();

        public abstract void SetSpeed(float newSpeed);

        /// <summary>
        /// Disable movement
        /// </summary>
        public abstract void Disable();

        /// <summary>
        /// Enable movement
        /// </summary>
        public abstract void Enable();

        public abstract float GetVelocity();

        public Action OnBeginMoving;
        public Action OnStopMoving;


        private Unit owner;

        public virtual void Initialize(float moveSpeed, Unit _owner)
        {
            SetSpeed(moveSpeed);
            owner = _owner;
        }



        public Action OnReachedDestination;

        protected virtual void Update()
        {
            if (!owner)
            {
                DisableCollision();
                return;
            }
            if (owner.IsDead)
            {
                DisableCollision();
                return;
            }
            if (lastVelocity <= 0)
            {
                if (GetVelocity() > 0)
                {
                    OnBeginMoving?.Invoke();
                }
            }
            else if (GetVelocity() <= 0)
            {
                OnStopMoving?.Invoke();
                //TODO stop hardcoding, calculate with stopping distance?
                if (Vector3.Distance(transform.position, TargetPos) <= 1.4f)
                {
                    OnReachedDestination?.Invoke();
                }
            }
            lastVelocity = GetVelocity();
            //TODO increase animation speed if running really fast
            if (owner.Animator)
            {
                owner.Animator.SetFloat("Speed", GetVelocity() / speed);
            }
        }

        /// <summary>
        /// Disables movement and all collision
        /// </summary>
        public abstract void DisableCollision();

        /// <summary>
        /// Enables movement and all collision
        /// </summary>
        public abstract void EnableCollision();

        /// <summary>
        /// Draws yellow line from self to targetPos
        /// </summary>
        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, TargetPos);
        }
    }
}