using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class FreezeRotation : MonoBehaviour
    {
        [SerializeField]
        private bool x;

        [SerializeField]
        private bool y;

        [SerializeField]
        private bool z;

        private Vector3 baseRotation;

        void Start()
        {
            baseRotation = transform.eulerAngles;
        }

        // Update is called once per frame
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
