using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MOBA
{

    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMovement : Movement
    {
        private NavMeshAgent agent;

        public override void Initialize(float moveSpeed)
        {
            agent = GetComponent<NavMeshAgent>();
            base.Initialize(moveSpeed);
        }

        public override void MoveTo(Vector3 destination)
        {
            agent.SetDestination(destination);
        }

        public override void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
            agent.speed = newSpeed;
        }

        private void Start()
        {
            OnBeginMoving += () => print("begin moving");
            OnStopMoving += () => print("stop moving");
        }

        public override void Stop()
        {
            agent.SetDestination(transform.position);
        }

        public override float GetVelocity()
        {
            return agent.velocity.magnitude;
        }
    }
}
