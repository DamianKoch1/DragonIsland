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

        private RectTransform rTransform;

        [SerializeField, Tooltip("(0, 0, 0) should be map middle, this is the distance from middle to any corner")]
        private float worldHalfDiameter = 75;
      
        private void Initialize()
        {
            rTransform = GetComponent<RectTransform>();
        }

        public bool IsCursorOnMinimap(out Vector3 minimapToWorldPos)
        {
            minimapToWorldPos = Vector3.zero;
            float width = rTransform.rect.width;
            float height = rTransform.rect.height;
            float mouseMinimapPosX = Input.mousePosition.x + width - Screen.width;
            float mouseMinimapPosY = -Input.mousePosition.y + height;

            if (mouseMinimapPosX < -width) return false;
            if (mouseMinimapPosY < -height) return false;

            float minimapScreenPosX = mouseMinimapPosX / width;
            float minimapScreenPosY = -mouseMinimapPosY / height;
            print(mouseMinimapPosX);

            minimapToWorldPos.x = 75 * minimapScreenPosX;
            minimapToWorldPos.z = 75 * minimapScreenPosY;
            return true;
        }
    }
}
