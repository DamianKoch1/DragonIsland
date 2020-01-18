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


        public LaneID Lane
        {
            private set;
            get;
        }

        public void Initialize(LaneID lane)
        {
            Lane = lane;
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
    }
}