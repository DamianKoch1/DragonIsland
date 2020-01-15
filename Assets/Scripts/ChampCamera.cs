using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class ChampCamera : MonoBehaviour
    {
        [SerializeField]
        private Champ target;
        private Vector3 offset;
        Quaternion rotation;

        void Start()
        {
            offset = transform.position - target.transform.position;
            rotation = transform.rotation;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            transform.position = target.transform.position + offset;
            transform.rotation = rotation;
        }

        public bool GetCursorToWorldPoint(out Vector3 result)
        {
            Ray ray = GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                result = hit.point;
                return true;
            }
            result = Vector3.zero;
            return false;
        }
    }
}
