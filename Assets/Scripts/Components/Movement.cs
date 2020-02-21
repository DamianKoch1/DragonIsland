using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{



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


        public virtual void MoveTo(Vector3 destination)
        {
            TargetPos = destination;
        }

        public abstract void Stop();

        public abstract void SetSpeed(float newSpeed);

        public abstract void Disable();

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
            if (owner.Animator)
            {
                owner.Animator.SetFloat("Speed", GetVelocity() / speed);
            }
        }


        public abstract void DisableCollision();

        public abstract void EnableCollision();

        protected void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, TargetPos);
        }
    }
}