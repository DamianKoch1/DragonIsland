using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MOBA
{
    /// <summary>
    /// Displays info of a room
    /// </summary>
    public class RoomView : MonoBehaviourPunCallbacks
    {
        private RoomInfo roomInfo;

        public string RoomName => roomInfo.Name;

        [SerializeField]
        private Text roomInfoText;

        [SerializeField]
        private Button joinButton;

        /// <summary>
        /// Updates room name, current / max players, IsOpen, visible, toggles joinButton depending on if it can be joined
        /// </summary>
        /// <param name="_roomInfo"></param>
        public void Refresh(RoomInfo _roomInfo)
        {
            roomInfo = _roomInfo;
            roomInfoText.text = roomInfo.ToString();
            joinButton.interactable = roomInfo.IsOpen && (roomInfo.PlayerCount < roomInfo.MaxPlayers || roomInfo.MaxPlayers == 0);
        }

        /// <summary>
        /// Client joins the room this view displays
        /// </summary>
        public void Join()
        {
            PhotonNetwork.JoinRoom(roomInfo.Name);
        }
    }
}
