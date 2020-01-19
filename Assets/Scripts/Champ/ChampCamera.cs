using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class ChampCamera : MonoBehaviour
    {
        private Champ target;
        private Vector3 offset;
        private Quaternion rotation;

        public void Initialize(Champ _target, Vector3 _offset, Quaternion _rotation)
        {
            target = _target;
            offset = _offset;
            rotation = _rotation;
        }

        void Update()
        {
            transform.position = target.transform.position + offset;
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
