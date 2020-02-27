using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MOBA
{
    /// <summary>
    /// Movement using a NavmeshAgent
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(NavMeshObstacle))]
    public class NavMovement : Movement
    {
        private NavMeshAgent agent;

        /// <summary>
        /// Disabling this turns agent off and obstacle on
        /// </summary>
        private NavMeshObstacle obstacle;

        public override void Initialize(float moveSpeed, Unit _owner)
        {
            agent = GetComponent<NavMeshAgent>();
            obstacle = GetComponent<NavMeshObstacle>();
            base.Initialize(moveSpeed, _owner);
        }

        /// <summary>
        /// Moves agent to destination if not already moving there
        /// </summary>
        /// <param name="destination">Target position</param>
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

        /// <summary>
        /// If source is not on NavMesh, tries to find closest position on NavMesh within an increasing range until one is found or until maxIterations is reached
        /// </summary>
        /// <param name="source">Position to look for a navigable position around</param>
        /// <returns>Returns closest navigable position if found, otherwise source</returns>
        public Vector3 ClosestNavigablePos(Vector3 source)
        {
            Vector3 result = source;
            result.y = 0;
            if (agent?.isOnNavMesh == true)
            {
                if (agent.CalculatePath(result, new NavMeshPath())) return source;
            }
            float currentMaxDist = 5;
            NavMeshHit hit = new NavMeshHit();
            int maxIterations = 20;
            int iterations = 0;
            while (iterations < maxIterations)
            {
                if (NavMesh.SamplePosition(result, out hit, currentMaxDist, NavMesh.AllAreas))
                {
                    if (Mathf.Abs(hit.position.y - source.y) <= 2)
                    {
                        break;
                    }
                }
                currentMaxDist *= 1.3f;
                iterations++;
            }
            if (hit.position != Vector3.zero)
            {
                result = hit.position;
            }
            else result = source;
            return result;
        }

        public override void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
            agent.speed = newSpeed;
        }


        /// <summary>
        /// Disables agent, enables obstacle
        /// </summary>
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

        /// <summary>
        /// Returns agent velocity
        /// </summary>
        /// <returns></returns>
        public override float GetVelocity()
        {
            return agent.velocity.magnitude;
        }

        /// <summary>
        /// Disables obstacle, enables agent
        /// </summary>
        public override void Enable()
        {
            obstacle.enabled = false;
            agent.enabled = true;
        }

        /// <summary>
        /// Disables collision, agent and obstacle
        /// </summary>
        public override void DisableCollision()
        {
            foreach (var collider in GetComponents<Collider>())
            {
                collider.enabled = false;
            }
            agent.enabled = false;
            obstacle.enabled = false;
        }

        /// <summary>
        /// Enables collision, agent, disables obstacle
        /// </summary>
        public override void EnableCollision()
        {
            foreach (var collider in GetComponents<Collider>())
            {
                collider.enabled = true;
            }
            obstacle.enabled = false;
            agent.enabled = true;
        }

        /// <summary>
        /// Stops agent (moves it to own position)
        /// </summary>
        public override void Stop()
        {
            MoveTo(transform.position);
        }
    }
}
