using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    /// <summary>
    /// A camera that follows the local player
    /// </summary>
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

        public float CurrentZoom { get; private set; } = 1;

        [SerializeField, Range(0.1f, 1), Tooltip("The smaller the value, the closer the camera can get.")]
        private float maxZoom = 0.5f;

        [SerializeField, Range(1, 10), Tooltip("The higher the value, the further away the camera can get.")]
        private float minZoom = 2f;

        /// <summary>
        /// Cursor position will be raycasted from this camera onto this plane
        /// </summary>
        public static Plane GroundPlane = new Plane(Vector3.up, Vector3.zero);

        [SerializeField]
        private KeyCode unlockKey;

        [SerializeField]
        private float unlockedMoveSpeed;

        public bool Unlocked { get; private set; }

        private Vector3 targetPos;

        [SerializeField, Tooltip("How close to screen edge in % of screen size the cursor has to be to move the camera when unlocked")]
        private float screenEdgeDistanceToMove = 0.1f;

        [SerializeField]
        private Vector3 blueSideLimit;

        [SerializeField]
        private Vector3 redSideLimit;

        /// <summary>
        /// Shows camera sight area on minimap
        /// </summary>
        private LineRenderer lr;

        private bool controllable = true;

        [Space, Header("Settings")]
        [SerializeField, Range(0.1f, 5)]
        private float scrollSpeed = 0.4f;

        /// <summary>
        /// Initializes variables
        /// </summary>
        /// <param name="_target">The champ to follow</param>
        /// <param name="_offset">The offset to keep from its position</param>
        /// <param name="_rotation">The rotation to keep</param>
        public void Initialize(Champ _target, Vector3 _offset, Quaternion _rotation)
        {
            distanceFactor = 1;
            CurrentZoom = 1;
            target = _target;
            offset = _offset;
            rotation = _rotation;
            cam = GetComponent<Camera>();
            lr = GetComponent<LineRenderer>();
        }

        void Update()
        {
            ProcessInput();
        }

        private void LateUpdate()
        {
            if (Unlocked)
            {
                UnlockedMovement();
            }
            else
            {
                targetPos = target.transform.position;
            }

            FollowTargetPos();

            VisualizeOnMinimap();
        }

        private void ProcessInput()
        {
            if (!controllable) return;
            if (Input.GetKeyDown(unlockKey))
            {
                ToggleLocked();
            }

            var scrollAxis = Input.GetAxis("Mouse ScrollWheel");
            if (scrollAxis != 0)
            {
                AddZoom(-scrollAxis * scrollSpeed);
            }
        }

        /// <summary>
        /// Moves camera if cursor is close to screen edges
        /// </summary>
        private void UnlockedMovement()
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

        /// <summary>
        /// Shows screen corners raycasted onto ground plane in the lineRenderer
        /// </summary>
        private void VisualizeOnMinimap()
        {
            ScreenToGroundPoint(Vector3.zero, out var result);
            lr.SetPosition(0, result + Vector3.up * 25);

            ScreenToGroundPoint(new Vector3(Screen.width, 0, 0), out result);
            lr.SetPosition(1, result + Vector3.up * 25);

            ScreenToGroundPoint(new Vector3(Screen.width, Screen.height, 0), out result);
            lr.SetPosition(2, result + Vector3.up * 25);

            ScreenToGroundPoint(new Vector3(0, Screen.height, 0), out result);
            lr.SetPosition(3, result + Vector3.up * 25);
        }

        /// <summary>
        /// Moves to targetPos, lerps to targeted zoom value
        /// </summary>
        private void FollowTargetPos()
        {
            targetPos.x = Mathf.Clamp(targetPos.x, blueSideLimit.x, redSideLimit.x);
            targetPos.z = Mathf.Clamp(targetPos.z, blueSideLimit.z, redSideLimit.z);

            distanceFactor = Mathf.Lerp(distanceFactor, CurrentZoom, 0.1f);
            transform.position = targetPos + offset * distanceFactor;
        }

        private void ToggleLocked()
        {
            if (!controllable) return;
            Unlocked = !Unlocked;
        }

        public void Lock()
        {
            if (!controllable) return;
            Unlocked = false;
        }

        public void Unlock()
        {
            if (!controllable) return;
            Unlocked = true;
        }

        public void DisableControls()
        {
            controllable = false;
        }

        public void EnableControls()
        {
            controllable = true;
        }

        /// <summary>
        /// Zooms in up to a maximum by the given value (zooms out if it is negative)
        /// </summary>
        /// <param name="amount">Zoom amount</param>
        public void AddZoom(float amount)
        {
            if (!controllable) return;
            CurrentZoom += amount;
            if (CurrentZoom > minZoom) CurrentZoom = minZoom;
            else if (CurrentZoom < maxZoom) CurrentZoom = maxZoom;
        }

        /// <summary>
        /// Sets target zoom to given value, ignoring limits
        /// </summary>
        /// <param name="amount">New zoom</param>
        public void SetZoom(float amount)
        {
            if (!controllable) return;
            CurrentZoom = amount;
        }

        /// <summary>
        /// Raycasts a point on screen onto the ground plane
        /// </summary>
        /// <param name="screenPoint">Screen point (usually mouse position)</param>
        /// <param name="result">Resulting point on ground plane</param>
        /// <returns>Should never return false as the ground plane is infinite</returns>
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

        /// <summary>
        /// Raycasts mouse position onto ground plane
        /// </summary>
        /// <param name="result">Resulting position on ground plane</param>
        /// <returns></returns>
        public bool GetCursorToGroundPoint(out Vector3 result)
        {
            return ScreenToGroundPoint(Input.mousePosition, out result);
        }
    }
}
