using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    /// <summary>
    /// Used to (partly) freeze rotation of objects (like AOE vfx attached to a unit which it shouldn't spin with)
    /// </summary>
    public class FreezeRotation : MonoBehaviour
    {
        [SerializeField]
        private bool x;

        [SerializeField]
        private bool y;

        [SerializeField]
        private bool z;

        private Vector3 baseRotation;

        /// <summary>
        /// Save start rotation
        /// </summary>
        void Start()
        {
            baseRotation = transform.eulerAngles;
        }

        /// <summary>
        /// Reset assigned parts of the rotation each frame
        /// </summary>
        void Update()
        {
            var resettedRotation = transform.eulerAngles;
            if (x)
            {
                resettedRotation.x = baseRotation.x;
            }
            if (y)
            {
                resettedRotation.y = baseRotation.y;
            }
            if (z)
            {
                resettedRotation.z = baseRotation.z;
            }
            transform.eulerAngles = resettedRotation;
        }
    }
}
