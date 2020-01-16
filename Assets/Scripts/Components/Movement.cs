using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MOBA
{

    [RequireComponent(typeof(NavMeshAgent))]
    public class Movement : MonoBehaviour
    {
        private NavMeshAgent agent;

        private float velocity;

        // Start is called before the first frame update
        public void Initialize(float moveSpeed)
        {
            agent = GetComponent<NavMeshAgent>();
            SetSpeed(moveSpeed);
        }

        public void SetDestination(Vector3 destination)
        {
            agent.SetDestination(destination);
        }

        public void SetSpeed(float newSpeed)
        {
            agent.speed = newSpeed;
        }

        private void Start()
        {
            OnBeginMoving += () => print("begin moving");
            OnStopMoving += () => print("stop moving");
        }

        private void Update()
        {
            if (velocity <= 0)
            {
                if (agent.velocity.magnitude > 0)
                {
                    OnBeginMoving?.Invoke();
                }
            }
            else if (agent.velocity.magnitude <= 0)
            {
                OnStopMoving?.Invoke();
            }
            velocity = agent.velocity.magnitude;
        }

        public Action OnBeginMoving;
        public Action OnStopMoving;

        public void Stop()
        {
            agent.SetDestination(transform.position);
        }

        public float GetVelocity() => velocity;
    }
}
