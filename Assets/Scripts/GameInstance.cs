using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstance : MonoBehaviour
{
    private static GameInstance instance;
    public static GameInstance Instance
    {
        private set
        {
            instance = value;
        }
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<GameInstance>();
            }
            return instance;
        }
    }

    private void Start()
    {
        if (Instance) Destroy(gameObject);
    }
}
