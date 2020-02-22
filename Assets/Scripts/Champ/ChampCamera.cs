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

        private static ChampCamera instance;
        public static ChampCamera Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType<ChampCamera>();
                }
                return instance;
            }
        }

        private float distanceFactor = 1;
        private float targetDistanceFactor = 1;

        [SerializeField, Range(0.1f, 1), Tooltip("The smaller the value, the closer the camera can get.")]
        private float maxZoom = 0.5f;

        [SerializeField, Range(1, 10), Tooltip("The higher the value, the further away the camera can get.")]
        private float minZoom = 2f;

        public static Plane GroundPlane = new Plane(Vector3.up, Vector3.zero);

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

        private LineRenderer lr;

        private bool controllable = true;

        public void Initialize(Champ _target, Vector3 _offset, Quaternion _rotation)
        {
            distanceFactor = 1;
            targetDistanceFactor = 1;
            target = _target;
            offset = _offset;
            rotation = _rotation;
            cam = GetComponent<Camera>();
            lr = GetComponent<LineRenderer>();
        }

        void Update()
        {
            if (!controllable) return;
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

            ScreenToGroundPoint(Vector3.zero, out var result);
            lr.SetPosition(0, result + Vector3.up * 10);

            ScreenToGroundPoint(new Vector3(Screen.width, 0, 0), out result);
            lr.SetPosition(1, result + Vector3.up * 10);

            ScreenToGroundPoint(new Vector3(Screen.width, Screen.height, 0), out result);
            lr.SetPosition(2, result + Vector3.up * 10);

            ScreenToGroundPoint(new Vector3(0, Screen.height, 0), out result);
            lr.SetPosition(3, result + Vector3.up * 10);
        }

        private void ToggleLocked()
        {
            if (!controllable) return;
            unlocked = !unlocked;
        }

        public void Lock()
        {
            if (!controllable) return;
            unlocked = false;
        }

        public void Unlock()
        {
            if (!controllable) return;
            unlocked = true;
        }

        public void DisableControls()
        {
            controllable = false;
        }

        public void EnableControls()
        {
            controllable = true;
        }

        public void AddDistanceFactor(float amount, bool ignoreLimits = false)
        {
            if (!controllable) return;
            targetDistanceFactor += amount;
            if (ignoreLimits) return;
            if (targetDistanceFactor > minZoom) targetDistanceFactor = minZoom;
            else if (targetDistanceFactor < maxZoom) targetDistanceFactor = maxZoom;
        }

        private bool ScreenToGroundPoint(Vector3 screenPoint, out Vector3 result)
        {
            Ray ray = cam.ScreenPointToRay(screenPoint);
            if (GroundPlane.Raycast(ray, out var hit))
            {
                result = ray.GetPoint(hit);
                return true;
            }
            result = Vector3.zero;
            return false;
        }

        public bool GetCursorToGroundPoint(out Vector3 result)
        {
            return ScreenToGroundPoint(Input.mousePosition, out result);
        }
    }
}
