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

        public override void Initialize(float moveSpeed, Unit _owner)
        {
            agent = GetComponent<NavMeshAgent>();
            obstacle = GetComponent<NavMeshObstacle>();
            base.Initialize(moveSpeed, _owner);
        }

        public override void MoveTo(Vector3 destination)
        {
            if (!agent.enabled) return;
            if (GetVelocity() > 0.1f)
            {
                if (Vector3.Distance(TargetPos, destination) < 0.1f) return;
            }
            base.MoveTo(destination);
            agent.SetDestination(ClosestNavigablePos(destination));
        }

        public Vector3 ClosestNavigablePos(Vector3 source)
        {
            Vector3 result = source;
            result.y = 0;
            if (agent.CalculatePath(result, new NavMeshPath())) return source;
            float currentMaxDist = 5;
            NavMeshHit hit;
            while (true)
            {
                if (NavMesh.SamplePosition(result, out hit, currentMaxDist, NavMesh.AllAreas))
                {
                    if (Mathf.Abs(hit.position.y - source.y) <= 1)
                    {
                        break;
                    }
                }
                currentMaxDist *= 1.3f;
            }
            result = hit.position;
            return result;
        }

        public override void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
            agent.speed = newSpeed;
        }


        public override void Disable()
        {
            agent.enabled = false;
            obstacle.enabled = true;
        }

        protected override void Update()
        {
            if (!agent) return;
            if (!agent.enabled) return;
            base.Update();
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

        public override void DisableCollision()
        {
            foreach (var collider in GetComponents<Collider>())
            {
                collider.enabled = false;
            }
            agent.enabled = false;
            obstacle.enabled = false;
        }

        public override void EnableCollision()
        {
            foreach (var collider in GetComponents<Collider>())
            {
                collider.enabled = true;
            }
            obstacle.enabled = false;
            agent.enabled = true;
        }

        public override void Stop()
        {
            MoveTo(transform.position);
        }
    }
}
