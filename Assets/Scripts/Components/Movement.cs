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
        // Start is called before the first frame update
        public void Initialize(float moveSpeed)
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;
        }

        public void SetDestination(Vector3 destination)
        {
            agent.SetDestination(destination);
        }
    }
}
