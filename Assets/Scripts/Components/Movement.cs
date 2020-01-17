using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Movement : MonoBehaviour
{
    public float lastVelocity
    {
        get;
        protected set;
    }

    [SerializeField]
    protected float speed;

    public abstract void MoveTo(Vector3 destination);

    public abstract void SetSpeed(float newSpeed);

    public abstract void Stop();

    public abstract float GetVelocity();

    public Action OnBeginMoving;
    public Action OnStopMoving;


    public virtual void Initialize(float moveSpeed)
    {
        SetSpeed(moveSpeed);
    }


    protected virtual void Update()
    {
        if (lastVelocity <= 0)
        {
            if (GetVelocity() > 0)
            {
                OnBeginMoving?.Invoke();
            }
        }
        else if (GetVelocity() <= 0)
        {
            OnStopMoving?.Invoke();
        }
        lastVelocity = GetVelocity();
    }
}
