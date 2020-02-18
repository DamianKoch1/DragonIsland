using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{

    public class MinimapIconToggler : MonoBehaviour
    {
        void Start()
        {
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}
