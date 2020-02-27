using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    /// <summary>
    /// Shows icons for each unit, shows local player path / camera sight area, can click on it to move
    /// </summary>
    public class Minimap : MonoBehaviour
    {
        private static Minimap instance;

        public static Minimap Instance
        {
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType<Minimap>();
                    instance.Initialize();
                }
                return instance;
            }
        }

        /// <summary>
        /// Canvas holding this minimap (to account for window size)
        /// </summary>
        private Transform canvasTransform;

        private RectTransform rTransform;

        [SerializeField, Tooltip("(0, 0, 0) should be map middle, this is the distance from middle to any corner")]
        private float worldHalfDiameter = 75;
      
        private void Initialize()
        {
            rTransform = GetComponent<RectTransform>();
            canvasTransform = GetComponentInParent<Canvas>().transform;
        }

        /// <summary>
        /// Checks if cursor is over minimap, calculates respective world ground position
        /// </summary>
        /// <param name="minimapToWorldPos">resulting mouse ground position</param>
        /// <returns>returns false if mouse is not over minimap</returns>
        public bool IsCursorOnMinimap(out Vector3 minimapToWorldPos)
        {
            minimapToWorldPos = Vector3.zero;

            //get minimap dimensions, account for canvas scale (window size)
            float width = rTransform.rect.width * canvasTransform.localScale.x;
            float height = rTransform.rect.height * canvasTransform.localScale.y;

            //check if mouse pos is closer to minimap screen corner than minimap size (is on minimap)
            float mouseMinimapPosX = Input.mousePosition.x + width - Screen.width;
            float mouseMinimapPosY = -Input.mousePosition.y + height;

            //is on minimap?
            if (mouseMinimapPosX < 0) return false;
            if (mouseMinimapPosY < 0) return false;

            //get corresponding position on minimap from -1 (bottom left) to 1 (top right);
            float minimapScreenPosX = mouseMinimapPosX / width * 2 - 1;
            float minimapScreenPosY = -mouseMinimapPosY / height * 2 + 1;

            //get corresponding world position
            minimapToWorldPos.x = 75 * minimapScreenPosX;
            minimapToWorldPos.z = 75 * minimapScreenPosY;

            return true;
        }
    }
}
