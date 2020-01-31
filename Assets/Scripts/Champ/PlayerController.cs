using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MOBA.Logging;

namespace MOBA
{

    public class CursorInfo
    {
        public Unit hovered;
        public Vector3 position;

        public CursorInfo()
        { }

        public CursorInfo(Unit _hovered, Vector3 _position)
        {
            hovered = _hovered;
            position = _position;
        }
    }

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
        private ChampCamera camPrefab;

        [SerializeField]
        private ChampCamera previewCam;


        [SerializeField]
        private Vector3 camOffset;

        [SerializeField]
        private Vector3 camRotation;

        [Space]
        [SerializeField]
        private Champ player;

        private PhotonView playerView;

        [SerializeField]
        private Champ player1;

        [SerializeField]
        private Champ player2;

        [SerializeField]
        private Interface ui;

        public Interface UI => ui;

        public static Champ Player => Instance.player;

        private Vector3 mousePos;

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
            Destroy(previewCam.gameObject);
            var cam = Instantiate(camPrefab).GetComponent<ChampCamera>();
            cam.Initialize(player, camOffset, Quaternion.Euler(camRotation));
            player.SetCamera(cam);
            ui?.Initialize(player);
            playerView = PhotonView.Get(player);
            playerView.RequestOwnership();
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
                playerView.RPC(nameof(player.OnAttackCommand), RpcTarget.MasterClient, hovered.GetViewID());
                GameLogger.Log(player, LogActionType.attack, mousePos, hovered);
            }
            else if (player.GetMouseWorldPos(out mousePos))
            {
                playerView.RPC(nameof(player.OnMoveCommand), RpcTarget.MasterClient, mousePos);
                GameLogger.Log(player, LogActionType.move, mousePos);
            }
        }

        private void OnMoveHeld()
        {
            if (hovered)
            {
                playerView.RPC(nameof(player.OnAttackCommand), RpcTarget.MasterClient, hovered.GetViewID());
                GameLogger.Log(player, LogActionType.attack, mousePos, hovered);
            }
            else if(player.GetMouseWorldPos(out mousePos))
            {
                playerView.RPC(nameof(player.OnMoveCommand), RpcTarget.MasterClient, mousePos);
                GameLogger.Log(player, LogActionType.move, mousePos);
            }
        }

        private void OnAttackMovePressed()
        {
            if (hovered)
            {
                if (hovered.Targetable)
                {
                    playerView.RPC(nameof(player.OnAttackCommand), RpcTarget.MasterClient, hovered.GetViewID());
                    GameLogger.Log(player, LogActionType.attack, mousePos, hovered);
                }
                return;
            }
            if (!player.GetMouseWorldPos(out mousePos)) return;
            Instantiate(atkMoveClickVfx, mousePos + Vector3.up * 0.2f, Quaternion.identity);
            var targets = player.GetTargetableEnemiesInAtkRange<Unit>(mousePos);
            switch (targets.Count())
            {
                case 0:
                    playerView.RPC(nameof(player.OnMoveCommand), RpcTarget.MasterClient, mousePos);
                    GameLogger.Log(player, LogActionType.move, mousePos);
                    break;
                case 1:
                    var target = targets[0];
                    playerView.RPC(nameof(player.OnAttackCommand), RpcTarget.MasterClient, targets[0].GetViewID());
                    GameLogger.Log(player, LogActionType.attack, mousePos, target);
                    break;
                default:
                    var closestTarget = targets.GetClosestUnitFrom<Unit>(mousePos);
                    playerView.RPC(nameof(player.OnAttackCommand), RpcTarget.MasterClient, closestTarget.GetViewID());
                    GameLogger.Log(player, LogActionType.attack, mousePos, closestTarget);
                    break;
            }
        }

       

        [PunRPC]
        private void MoveChampTo(int ownerID, Vector3 targetPos)
        {
            var champ = (Champ)(ownerID.GetUnitByID());
            if (champ.IsAttacking())
            {
                champ.StopAttacking();
            }
            champ.MoveTo(targetPos);
        }

        [PunRPC]
        private void Attack(int ownerID, int targetID)
        {
            ((Champ)(ownerID.GetUnitByID())).StartAttacking(targetID.GetUnitByID());
        }


        private void ProcessPlayerInput()
        {
            if (player.IsDead) return;
            if (Input.GetMouseButtonDown(1))
            {
                OnMovePressed();
                Instantiate(moveClickVfx, mousePos + Vector3.up * 0.2f, Quaternion.identity);
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
                if (player.GetMouseWorldPos(out mousePos))
                {
                    playerView.RPC(nameof(player.CastQ), RpcTarget.All, hovered.GetViewID(), mousePos);
                    GameLogger.Log(player, LogActionType.Q, mousePos, hovered);
                }
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                //CastW(player);
                if (player.GetMouseWorldPos(out mousePos))
                {
                    playerView.RPC(nameof(player.CastW), RpcTarget.All, hovered.GetViewID(), mousePos);
                    GameLogger.Log(player, LogActionType.W, mousePos, hovered);
                }
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                //CastE(player);
                if (player.GetMouseWorldPos(out mousePos))
                {
                    playerView.RPC(nameof(player.CastE), RpcTarget.All, hovered.GetViewID(), mousePos);
                    GameLogger.Log(player, LogActionType.E, mousePos, hovered);
                }
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                //CastR(player);
                if (player.GetMouseWorldPos(out mousePos))
                {
                    playerView.RPC(nameof(player.CastR), RpcTarget.MasterClient, hovered.GetViewID(), mousePos);
                    GameLogger.Log(player, LogActionType.R, mousePos, hovered);
                }
            }
        }

        private void ProcessCamInput()
        {
            var scrollAxis = Input.GetAxis("Mouse ScrollWheel");
            if (scrollAxis != 0)
            {
                camPrefab.AddDistanceFactor(-scrollAxis * scrollSpeed);
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
            if (!previewCam) return;
            previewCam.transform.position = player.transform.position + camOffset;
            previewCam.transform.rotation = Quaternion.Euler(camRotation);
        }


    }

}


