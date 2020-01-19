﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MOBA
{
    public class ProjectileMovement : Movement
    {
        protected bool stopped = false;

        protected Vector3 lastPos;

        public override void Enable()
        {
            throw new System.NotImplementedException();
        }

        public override float GetVelocity()
        {
            return Vector3.Distance(transform.position, lastPos) / Time.deltaTime;
        }

        public override void MoveTo(Vector3 destination)
        {
            if (stopped) return;
            base.MoveTo(destination);
            transform.LookAt(destination);
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        public override void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }

        public override void Disable()
        {
            stopped = true;
        }

        protected override void Update()
        {
            base.Update();

            lastPos = transform.position;
        }
    }
}
