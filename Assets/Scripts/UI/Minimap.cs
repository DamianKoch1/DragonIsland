using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{

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

        private Transform canvasTransform;

        private RectTransform rTransform;

        [SerializeField, Tooltip("(0, 0, 0) should be map middle, this is the distance from middle to any corner")]
        private float worldHalfDiameter = 75;
      
        private void Initialize()
        {
            rTransform = GetComponent<RectTransform>();
            canvasTransform = GetComponentInParent<Canvas>().transform;
        }

        public bool IsCursorOnMinimap(out Vector3 minimapToWorldPos)
        {
            minimapToWorldPos = Vector3.zero;

            //account for canvas scale (window size)
            float width = rTransform.rect.width * canvasTransform.localScale.x;
            float height = rTransform.rect.height * canvasTransform.localScale.y;

            //check if mouse pos is closer to minimap corner than minimap size (is on minimap)
            float mouseMinimapPosX = Input.mousePosition.x + width - Screen.width;
            float mouseMinimapPosY = -Input.mousePosition.y + height;
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
