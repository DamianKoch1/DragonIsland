using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{

    public class PlayerController : StatBarHUD<Champ>
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

        public static Champ Player => Instance.target;

        [SerializeField]
        private Image XPBar;

        [SerializeField]
        private Text LevelText;

        public DefaultColors defaultColors;

        public Shader outline;

        [SerializeField]
        private ParticleSystem moveClickVfx;

        [SerializeField]
        private ParticleSystem atkMoveClickVfx;


        [Header("Keybinds")]

        [SerializeField]
        private KeyCode attackMove;

        protected override void Initialize()
        {
            if (instance && instance != this) Destroy(gameObject);
            base.Initialize();
            XPBar.fillAmount = 0;
            LevelText.text = "1";
            target.OnXPChanged += SetXP;
            target.OnLevelUp += SetLvl;
            cam.Initialize(target, camOffset, Quaternion.Euler(camRotation));
        }

        protected void SetXP(float newAmount, float max)
        {
            XPBar.fillAmount = newAmount / max;
        }

        protected void SetLvl(int newLvl)
        {
            LevelText.text = newLvl + "";
        }

        private void DisplayStats(Unit target)
        {

        }

        private void OnSelectPressed()
        {
            if (!hovered) return;
            DisplayStats(hovered);
        }

        private void OnMovePressed()
        {
            if (hovered)
            {
                AttackHovered();
            }
            else if (MoveToCursor(out var targetPos))
            {
                target.attackMoving = false;
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
                AttackHovered();
                return;
            }
            if (!cam.GetCursorToWorldPoint(out var worldMousePos)) return;
            Instantiate(atkMoveClickVfx, worldMousePos + Vector3.up * 0.2f, Quaternion.identity);
            var targets = target.GetTargetableEnemiesInRange(worldMousePos);
            switch (targets.Count)
            {
                case 0:
                    target.attackMoving = true;
                    MoveToCursor(out var _);
                    break;
                case 1:
                    target.StartAttacking(targets[0]);
                    break;
                default:
                    target.StartAttacking(Unit.GetClosest(targets, worldMousePos));
                    break;
            }
        }



        private bool MoveToCursor(out Vector3 targetPos)
        {
            if (target.IsAttacking())
            {
                target.StopAttacking();
            }
            if (cam.GetCursorToWorldPoint(out var worldMousePos))
            {
                target.MoveTo(worldMousePos);
                targetPos = worldMousePos;
                return true;
            }
            targetPos = Vector3.zero;
            return false;
        }

        private void AttackHovered()
        {
            target.StartAttacking(hovered);
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
            }
        }

        private void OnValidate()
        {
            if (!cam) return;
            cam.transform.position = target.transform.position + camOffset;
            cam.transform.rotation = Quaternion.Euler(camRotation);
        }

    }
}
