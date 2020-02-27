using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    /// <summary>
    /// Used to mark the path minions use to walk their lanes down
    /// </summary>
    public class LaneWaypoint : MonoBehaviour
    {
        [SerializeField]
        private LaneWaypoint next;

        private LaneWaypoint prev;


        private LaneID lane;

        /// <summary>
        /// Saves give laneID, initializes next waypoint if assigned and sets its previous one to this
        /// </summary>
        /// <param name="lane"></param>
        public void Initialize(LaneID lane)
        {
            this.lane = lane;
            if (next)
            {
                next.prev = this;
                next.Initialize(lane);
            }
        }

        /// <summary>
        /// Returns position of next waypoint, if no next one returns red base position
        /// </summary>
        /// <returns></returns>
        public Vector3 GetNextPosition()
        {
            if (!next) return Base.InstanceRed.transform.position;
            return next.transform.position;
        }

        /// <summary>
        /// Returns position of previous waypoint, if no previous one returns blue base position
        /// </summary>
        /// <returns></returns>
        public Vector3 GetPrevPosition()
        {
            if (!prev) return Base.InstanceBlue.transform.position;
            return prev.transform.position;
        }

        /// <summary>
        /// Calls OnReachedWaypoint for entering minions of the same lane as this
        /// </summary>
        /// <param name="other"></param>
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