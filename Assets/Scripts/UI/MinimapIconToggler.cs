using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    /// <summary>
    /// Used to enable minimap icon sprites at start (which are disabled in editor to reduce visual clutter)
    /// </summary>
    public class MinimapIconToggler : MonoBehaviour
    {
        void Start()
        {
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}
