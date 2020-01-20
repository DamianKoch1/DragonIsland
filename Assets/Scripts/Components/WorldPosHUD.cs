using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MOBA
{
    public class WorldPosHUD : MonoBehaviour
    {
        private Unit target;

        [SerializeField]
        private Transform HUD;

        private float yOffset = 0;

        public void Initialize(Unit _target, float _yOffset)
        {
            target = _target;
            yOffset = _yOffset;
            HUD.position = Camera.main.WorldToScreenPoint(target.transform.position + Vector3.up * yOffset);
        }

        private void LateUpdate()
        {
            Vector3 targetPos = Camera.main.WorldToScreenPoint(target.transform.position + Vector3.up * yOffset);
            HUD.position = targetPos;
        }

    }
}
