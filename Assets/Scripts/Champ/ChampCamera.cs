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

        private Camera cam;

        private float distanceFactor = 1;
        private float targetDistanceFactor = 1;

        [SerializeField, Range(0.1f, 1), Tooltip("The smaller the value, the closer the camera can get.")]
        private float maxZoom = 0.5f;

        [SerializeField, Range(1, 10), Tooltip("The higher the value, the further away the camera can get.")]
        private float minZoom = 2f;

        Plane groundPlane;

        [SerializeField]
        private KeyCode unlockKey;

        [SerializeField]
        private float unlockedMoveSpeed;

        private bool unlocked;

        private Vector3 targetPos;

        [SerializeField, Tooltip("How close to screen edge in % of screen size the cursor has to be to move the camera when unlocked")]
        private float screenEdgeDistanceToMove = 0.1f;

        [SerializeField]
        private Vector3 blueSideLimit;

        [SerializeField]
        private Vector3 redSideLimit;

        public void Initialize(Champ _target, Vector3 _offset, Quaternion _rotation)
        {
            distanceFactor = 1;
            targetDistanceFactor = 1;
            target = _target;
            offset = _offset;
            rotation = _rotation;
            groundPlane = new Plane(Vector3.up, Vector3.zero);
            cam = GetComponent<Camera>();
        }

        void Update()
        {

            if (Input.GetKeyDown(unlockKey))
            {
                ToggleLocked();
            }

            if (unlocked)
            {
                var mouseVPPos = cam.ScreenToViewportPoint(Input.mousePosition);
                if (mouseVPPos.x > 1 - screenEdgeDistanceToMove)
                {
                    targetPos += Vector3.right * unlockedMoveSpeed;
                }
                else if (mouseVPPos.x < screenEdgeDistanceToMove)
                {
                    targetPos += -Vector3.right * unlockedMoveSpeed;
                }

                if (mouseVPPos.y > 1 - screenEdgeDistanceToMove)
                {
                    targetPos += Vector3.forward * unlockedMoveSpeed;
                }
                else if (mouseVPPos.y < screenEdgeDistanceToMove)
                {
                    targetPos += -Vector3.forward * unlockedMoveSpeed;
                }
            }
            else
            {
                targetPos = target.transform.position;
            }
            targetPos.x = Mathf.Clamp(targetPos.x, blueSideLimit.x, redSideLimit.x);
            targetPos.z = Mathf.Clamp(targetPos.z, blueSideLimit.z, redSideLimit.z);
            distanceFactor = Mathf.Lerp(distanceFactor, targetDistanceFactor, 0.1f);
            transform.position = targetPos + offset * distanceFactor;
        }

        private void ToggleLocked()
        {
            unlocked = !unlocked;
        }

        public void AddDistanceFactor(float amount)
        {
            targetDistanceFactor += amount;
            if (targetDistanceFactor > minZoom) targetDistanceFactor = minZoom;
            else if (targetDistanceFactor < maxZoom) targetDistanceFactor = maxZoom;
        }

        public bool GetCursorToWorldPoint(out Vector3 result)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
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
