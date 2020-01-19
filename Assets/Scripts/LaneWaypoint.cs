using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class LaneWaypoint : MonoBehaviour
    {
        [SerializeField]
        private LaneWaypoint next;

        private LaneWaypoint prev;


        private LaneID lane;

        public void Initialize(LaneID lane)
        {
            this.lane = lane;
            if (next)
            {
                next.prev = this;
                next.Initialize(lane);
            }
        }

        public Vector3 GetNextPosition()
        {
            if (!next) return Base.InstanceRed.transform.position;
            return next.transform.position;
        }

        public Vector3 GetPrevPosition()
        {
            if (!prev) return Base.InstanceBlue.transform.position;
            return prev.transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger) return;
            var minion = other.GetComponent<Minion>();
            if (minion)
            {
                if (minion.TargetLane == lane)
                {
                    minion.OnReachedWaypoint(this);
                }
            }
        }
    }
}