using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MOBA
{
    /// <summary>
    /// Component for UI elements that should be at world to screen position of target unit
    /// </summary>
    public class WorldPosHUD : MonoBehaviour
    {
        private Unit target;

        [SerializeField]
        private Transform HUD;

        private float yOffset = 0;

        /// <summary>
        /// Initializes position
        /// </summary>
        /// <param name="_target">Target to follow</param>
        /// <param name="_yOffset">Y offset to keep from target</param>
        public void Initialize(Unit _target, float _yOffset)
        {
            target = _target;
            yOffset = _yOffset;
            HUD.position = Camera.main.WorldToScreenPoint(target.transform.position + Vector3.up * yOffset);
        }

        /// <summary>
        /// Adjusts position to target world to screen position, destroys this object if target is destroyed
        /// </summary>
        private void LateUpdate()
        {
            if (!target)
            {
                Destroy(gameObject);
                return;
            }
            Vector3 targetPos = Camera.main.WorldToScreenPoint(target.transform.position + Vector3.up * yOffset);
            HUD.position = targetPos;
        }

    }
}
