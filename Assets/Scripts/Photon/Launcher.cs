using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Launcher : MonoBehaviourPunCallbacks
{
    string gameVersion = "1";
    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        Connect();
    }

    [SerializeField]
    private Text roomDisplay;

    [SerializeField]
    private Button startBtn;


    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        print(cause);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
        //PhotonNetwork.JoinRoom("room");
    }


    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(Guid.NewGuid().ToString(), new RoomOptions() { MaxPlayers = 10 });
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom("room", new RoomOptions() { MaxPlayers = 10 });
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient) startBtn.interactable = true;
        else startBtn.GetComponentInChildren<Text>().text = "Only masterClient can start!";

        roomDisplay.text = PhotonNetwork.CurrentRoom.ToString();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomDisplay.text = PhotonNetwork.CurrentRoom.ToString();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomDisplay.text = PhotonNetwork.CurrentRoom.ToString();
    }

    public void TryLoadLevel()
    {
        if (!PhotonNetwork.InRoom) return;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel("Game");
    }

}
