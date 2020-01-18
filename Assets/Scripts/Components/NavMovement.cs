using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MOBA
{

    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(NavMeshObstacle))]
    public class NavMovement : Movement
    {
        private NavMeshAgent agent;

        private NavMeshObstacle obstacle;

        public override void Initialize(float moveSpeed)
        {
            agent = GetComponent<NavMeshAgent>();
            obstacle = GetComponent<NavMeshObstacle>();
            base.Initialize(moveSpeed);
        }

        public override void MoveTo(Vector3 destination)
        {
            if (!agent.enabled) return;
            agent.SetDestination(destination);
        }

        public override void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
            agent.speed = newSpeed;
        }

        private void Start()
        {
        }

        public override void Disable()
        {
            agent.enabled = false;
            obstacle.enabled = true;
        }

        public override float GetVelocity()
        {
            return agent.velocity.magnitude;
        }

        public override void Enable()
        {
            obstacle.enabled = false;
            agent.enabled = true;
        }
    }
}
