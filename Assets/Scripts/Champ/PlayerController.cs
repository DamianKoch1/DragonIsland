﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{

    public class PlayerController : MonoBehaviour
    {
        private static PlayerController instance;

        public static PlayerController Instance
        {
            set => instance = value;
            get
            {
                if (!instance)
                {
                    instance = FindObjectOfType<PlayerController>();
                }
                return instance;
            }
        }

        [HideInInspector]
        public Unit hovered;

        [SerializeField]
        private ChampCamera cam;


        [SerializeField]
        private Vector3 camOffset;

        [SerializeField]
        private Vector3 camRotation;

        [Space]
        [SerializeField]
        private Champ player;

        [SerializeField]
        private Interface UI;

        public static Champ Player => Instance.player;


        [Space]
        public DefaultColors defaultColors;

        public Shader outline;

        [SerializeField]
        private ParticleSystem moveClickVfx;

        [SerializeField]
        private ParticleSystem atkMoveClickVfx;

        [Header("Settings")]

        [SerializeField, Range(0.1f, 5)]
        private float scrollSpeed = 0.4f;

        [Header("Keybinds")]

        [SerializeField]
        private KeyCode attackMove;

        private void Start()
        {
            if (instance && instance != this) Destroy(gameObject);
           
            cam.Initialize(player, camOffset, Quaternion.Euler(camRotation));
            UI?.Initialize(player);
        }



        private void OnSelectPressed()
        {
            if (!hovered)
            {
                UI.HideStats();
                return;
            }
            UI.ShowTargetStats(hovered);
        }

        private void OnMovePressed()
        {
            if (hovered)
            {
                AttackHovered();
            }
            else if (MoveToCursor(out var targetPos))
            {
                Instantiate(moveClickVfx, targetPos + Vector3.up * 0.2f, Quaternion.identity);
            }
        }

        private void OnMoveHeld()
        {
            if (hovered)
            {
                AttackHovered();
            }
            else
            {
                MoveToCursor(out var _);
            }
        }

        private void OnAttackMovePressed()
        {
            if (hovered)
            {
                if (hovered.Targetable)
                {
                    AttackHovered();
                }
                return;
            }
            if (!cam.GetCursorToWorldPoint(out var worldMousePos)) return;
            Instantiate(atkMoveClickVfx, worldMousePos + Vector3.up * 0.2f, Quaternion.identity);
            var targets = player.GetTargetableEnemiesInAtkRange(worldMousePos);
            switch (targets.Count)
            {
                case 0:
                    MoveToCursor(out var _);
                    break;
                case 1:
                    player.StartAttacking(targets[0]);
                    break;
                default:
                    player.StartAttacking(Unit.GetClosestUnitFrom(targets, worldMousePos));
                    break;
            }
        }



        private bool MoveToCursor(out Vector3 targetPos)
        {
            if (player.IsAttacking())
            {
                player.StopAttacking();
            }
            if (cam.GetCursorToWorldPoint(out var worldMousePos))
            {
                player.MoveTo(worldMousePos);
                targetPos = worldMousePos;
                return true;
            }
            targetPos = Vector3.zero;
            return false;
        }

        private void AttackHovered()
        {
            player.StartAttacking(hovered);
        }


        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                OnMovePressed();
            }
            else if (Input.GetMouseButton(1))
            {
                OnMoveHeld();
            }

            if (Input.GetMouseButtonDown(0))
            {
                OnSelectPressed();
            }

            if (Input.GetKeyDown(attackMove))
            {
                OnAttackMovePressed();
                player.ToggleRangeIndicator(true);
            }
            else if (Input.GetKeyUp(attackMove))
            {
                player.ToggleRangeIndicator(false);
            }

            var scrollAxis = Input.GetAxis("Mouse ScrollWheel");
            if (scrollAxis != 0)
            {
                cam.AddDistanceFactor(-scrollAxis * scrollSpeed);
            }

            //debug
            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                if (Time.timeScale < 8)
                {
                    Time.timeScale *= 2;
                }
            }
            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                if (Time.timeScale > 0.25f)
                {
                    Time.timeScale /= 2;
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                player.DebugMode();
            }

        }

        private void OnValidate()
        {
            if (!cam) return;
            cam.transform.position = player.transform.position + camOffset;
            cam.transform.rotation = Quaternion.Euler(camRotation);
        }

    }
}
