using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
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
                    instance.Initialize();
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
        private Champ player1;

        [SerializeField]
        private Champ player2;

        [SerializeField]
        private Interface ui;

        public Interface UI => ui;

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
            if (Instance && Instance != this) Destroy(gameObject);
        }

        public void Initialize()
        {
            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    player = player1;
                }
                else player = player2;
            }
            cam.Initialize(player, camOffset, Quaternion.Euler(camRotation));
            ui?.Initialize(player);
        }

        public bool GetMouseWorldPos(out Vector3 mouseWorldPos)
        {
            return cam.GetCursorToWorldPoint(out mouseWorldPos);
        }

        private void OnSelectPressed()
        {
            if (!hovered)
            {
                ui.HideTargetStats();
                return;
            }
            ui.ShowTargetStats(hovered);
        }

        private void OnMovePressed()
        {
            if (hovered)
            {
                //Attack(player, hovered);
                PhotonView.Get(this).RPC(nameof(Attack), RpcTarget.All, GetViewID(player), GetViewID(hovered));
            }
            else if (GetMouseWorldPos(out var targetPos))
            {
                //MoveChampTo(player, targetPos);
                PhotonView.Get(this).RPC(nameof(MoveChampTo), RpcTarget.All, GetViewID(player), targetPos);

                Instantiate(moveClickVfx, targetPos + Vector3.up * 0.2f, Quaternion.identity);
            }
        }

        private void OnMoveHeld()
        {
            if (hovered)
            {
                //Attack(player, hovered);
                PhotonView.Get(this).RPC(nameof(Attack), RpcTarget.All, GetViewID(player), GetViewID(hovered));
            }
            else if (GetMouseWorldPos(out var targetPos))
            {
                //MoveChampTo(player, targetPos);
                PhotonView.Get(this).RPC(nameof(MoveChampTo), RpcTarget.All, GetViewID(player), targetPos);
            }
        }

        private void OnAttackMovePressed()
        {
            if (hovered)
            {
                if (hovered.Targetable)
                {
                    //Attack(player, hovered);
                    PhotonView.Get(this).RPC(nameof(Attack), RpcTarget.All, GetViewID(player), GetViewID(hovered));
                }
                return;
            }
            if (!cam.GetCursorToWorldPoint(out var mouseWorldPos)) return;
            Instantiate(atkMoveClickVfx, mouseWorldPos + Vector3.up * 0.2f, Quaternion.identity);
            var targets = player.GetTargetableEnemiesInAtkRange<Unit>(mouseWorldPos);
            switch (targets.Count())
            {
                case 0:
                    //MoveChampTo(player, targetPos);
                    PhotonView.Get(this).RPC(nameof(MoveChampTo), RpcTarget.All, GetViewID(player), mouseWorldPos);
                    break;
                case 1:
                    //Attack(player, targets[0]);
                    PhotonView.Get(this).RPC(nameof(Attack), RpcTarget.All, GetViewID(player), GetViewID(targets[0]));
                    break;
                default:
                    //Attack(player, targets.GetClosestUnitFrom<Unit>(mouseWorldPos));
                    PhotonView.Get(this).RPC(nameof(Attack), RpcTarget.All, GetViewID(player), GetViewID(targets.GetClosestUnitFrom<Unit>(mouseWorldPos)));
                    break;
            }
        }

        private int GetViewID(Unit from)
        {
            return PhotonView.Get(from).ViewID;
        }

        private Unit GetUnitByID(int viewID)
        {
            return PhotonView.Find(viewID).GetComponent<Unit>();
        }

        [PunRPC]
        private void MoveChampTo(int ownerID, Vector3 targetPos)
        {
            var champ = (Champ)GetUnitByID(ownerID);
            if (champ.IsAttacking())
            {
                champ.StopAttacking();
            }
            champ.MoveTo(targetPos);
        }

        [PunRPC]
        private void Attack(int ownerID, int targetID)
        {
            ((Champ)GetUnitByID(ownerID)).StartAttacking(GetUnitByID(targetID));
        }


        //TODO add cast fail feedback
        [PunRPC]
        private void CastQ(int ownerID)
        {
            ((Champ)GetUnitByID(ownerID)).CastQ();
        }

        [PunRPC]
        private void CastW(int ownerID)
        {
            ((Champ)GetUnitByID(ownerID)).CastW();
        }

        [PunRPC]
        private void CastE(int ownerID)
        {
            ((Champ)GetUnitByID(ownerID)).CastE();
        }

        [PunRPC]
        private void CastR(int ownerID)
        {
            ((Champ)GetUnitByID(ownerID)).CastR();
        }


        private void ProcessPlayerInput()
        {
            if (player.IsDead) return;
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

            if (Input.GetKeyDown(KeyCode.Q))
            {
                //CastQ(player);
                PhotonView.Get(this).RPC(nameof(CastQ), RpcTarget.All, GetViewID(player));
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                //CastW(player);
                PhotonView.Get(this).RPC(nameof(CastW), RpcTarget.All, GetViewID(player));
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                //CastE(player);
                PhotonView.Get(this).RPC(nameof(CastE), RpcTarget.All, GetViewID(player));
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                //CastR(player);
                PhotonView.Get(this).RPC(nameof(CastR), RpcTarget.All, GetViewID(player));
            }
        }

        private void ProcessCamInput()
        {
            var scrollAxis = Input.GetAxis("Mouse ScrollWheel");
            if (scrollAxis != 0)
            {
                cam.AddDistanceFactor(-scrollAxis * scrollSpeed);
            }
        }

        private void ProcessDebugInput()
        {
            if (PhotonNetwork.IsConnected)
            {
                if (!PhotonNetwork.IsMasterClient) return;
            }
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
                player.Stats.DebugMode();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                player.LevelUp();
            }
        }



        private void Update()
        {
            ProcessPlayerInput();
            ProcessCamInput();
            ProcessDebugInput();
        }

        private void OnValidate()
        {
            if (!cam) return;
            cam.transform.position = player.transform.position + camOffset;
            cam.transform.rotation = Quaternion.Euler(camRotation);
        }


    }

}


