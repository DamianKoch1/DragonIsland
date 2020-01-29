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

        private float distanceFactor = 1;
        private float targetDistanceFactor = 1;

        [SerializeField, Range(0.1f, 1), Tooltip("The smaller the value, the closer the camera can get.")]
        private float maxZoom = 0.5f;

        [SerializeField, Range(1, 10), Tooltip("The higher the value, the further away the camera can get.")]
        private float minZoom = 2f;

        Plane groundPlane;

        public void Initialize(Champ _target, Vector3 _offset, Quaternion _rotation)
        {
            distanceFactor = 1;
            targetDistanceFactor = 1;
            target = _target;
            offset = _offset;
            rotation = _rotation;
            groundPlane = new Plane(Vector3.up, Vector3.zero);
        }

        void Update()
        {
            transform.position = target.transform.position + offset * distanceFactor;
            distanceFactor = Mathf.Lerp(distanceFactor, targetDistanceFactor, 0.1f);
        }

        public void AddDistanceFactor(float amount)
        {
            targetDistanceFactor += amount;
            if (targetDistanceFactor > minZoom) targetDistanceFactor = minZoom;
            else if (targetDistanceFactor < maxZoom) targetDistanceFactor = maxZoom;
        }

        public bool GetCursorToWorldPoint(out Vector3 result)
        {
            Ray ray = GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
            if (groundPlane.Raycast(ray, out var hit))
            {
                result = ray.GetPoint(hit);
                return true;
            }
            result = Vector3.zero;
            return false;
        }
    }
}
