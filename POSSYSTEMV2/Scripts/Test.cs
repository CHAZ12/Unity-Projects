
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using VRC.Udon.Common.Interfaces;

namespace VRCPOSSYSTEM
{
    public class Test : UdonSharpBehaviour
    {
        private int localPlayerID = -1;

        public VRCPlayerApi localPlayer;
        public int networkingLocalPlayerID;
        public TextMeshProUGUI player1ScoreText;
        public TextMeshProUGUI player2ScoreText;

        public int[] ids = null;
        public TextMeshProUGUI player1MenuText;
        public TextMeshProUGUI player2MenuText;
        private float SCA = 20000.00f; // Starting customer amount
        private float SBA = 40000.00f; //Starting bar Amount

        public bool isSignedUpToPlay;
        //SYNCED VAR
        [UdonSynced]
        public int player1ID;
        [UdonSynced]
        public int player2ID;
        [UdonSynced] 
        private float barAmount = 0;
        [UdonSynced]
        private int CusAmount;
        [UdonSynced] 
        public bool cusActive = false;
        [UdonSynced] 
        public bool barActive = false;
        [UdonSynced]
        public float[] AmountArray;
        // public PosSystemV2 manager;

        public void Start()
        {
            if (Networking.LocalPlayer == null) return;
            localPlayer = Networking.LocalPlayer;
            networkingLocalPlayerID = localPlayer.playerId;
        }
        private void InitializeIdsIfNull()
        {
            if (ids == null)
            {
                ids = new int[80];
                for (int i = 0; i < ids.Length; i++)
                {
                    // Assuming that the player ID does not contain -1, leave -1 blank. 
                    ids[i] = -1;
                }
            }
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            InitializeIdsIfNull();

            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] == -1)
                {
                    ids[i] = player.playerId;
                    break;
                }
            }
            Start();
            UpdateArray();
        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            InitializeIdsIfNull();

            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] == player.playerId)
                {
                    ids[i] = -1;
                    break;
                }
            }
            UpdateArray();

        }
        // Create a new length of an array becuae lists is not supported yet ;( (Note need 2 arrays inorder to get values from player joining or leaving)
        private void UpdateArray()
        {
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] != -1)
                {
                    AmountArray = new float[i+1];
                }
            }
        }
        private void JoinGame(int playerNumber)
        {
            Networking.SetOwner(localPlayer, gameObject);
            localPlayerID = playerNumber;

            switch (playerNumber)
            {
                case 0:
                    player1ID = networkingLocalPlayerID;
                    Debug.Log(player1ID);
                    break;
                case 1:
                    player2ID = networkingLocalPlayerID;
                    Debug.Log(player2ID);
                    break;
                default:
                    return;
            }
            DoStuff();
        }
        // Update displays for users
        public void UpdateMainMenu(int player1ID, int player2ID)
        {
            bool found = false;
            if (player1ID > 0)
            {
                int PID = 0;
                found = HandlePlayerState(player1MenuText, player1ScoreText, VRCPlayerApi.GetPlayerById(player1ID), PID);
            }
            else
            if (player2ID > 0)
            {
                int PID = 1;
                found = HandlePlayerState(player2MenuText, player2ScoreText, VRCPlayerApi.GetPlayerById(player2ID), PID);
                
            }
            else

            if (!found) Debug.LogWarning("NO PLAYERS");
        }
        public bool HandlePlayerState(TextMeshProUGUI menuText, TextMeshProUGUI scoreText, VRCPlayerApi player, int PID)
        {
           // if (PID == 1) player.SetPlayerTag("bob", 1.ToString());
           // else player.SetPlayerTag("bob", 2.ToString());
           // menuText.text = player.displayName;
            scoreText.text = player.displayName + "p1: " + player1ID + ", p2: " + player2ID + ", BOB:" + AmountArray[PID];
            menuText.text = "";
            for (int i = 0; i < AmountArray.Length; i++) menuText.text += "PlayerID: " + i + " " + player.displayName + ", " + AmountArray[i].ToString() + localPlayer.playerId + ",\n ";

            if (player.playerId == Networking.LocalPlayer.playerId) return true;
            return false;
        }
        public void _PlusCus()
        {
            if (cusActive == false)
            {
                Networking.SetOwner(localPlayer, gameObject);
                if (localPlayer == null) return;
                {
                    localPlayer.SetPlayerTag("CusAmount", 10.ToString());
                    CusAmount = Int32.Parse(localPlayer.GetPlayerTag("CusAmount"));
                   // cusActive = true;
                    AmountArray[localPlayer.playerId + 1] = CusAmount;
                    Debug.LogWarning("PlusCus Fired");
                    JoinGame(0);
                    //boolBarAccept = false;
                }
            }
            else Debug.LogWarning("someones else is using Cus");
        }
        public void _PlusBar()// Start settings values for Bar
        {
            if (barActive == false)
            {
                Networking.SetOwner(localPlayer, gameObject);
                if (localPlayer == null) return;
                { 
                    Debug.LogWarning("PlusBar Fired");
                   // barActive = true;
                    localPlayer.SetPlayerTag("CusAmount", 5.ToString());
                    CusAmount = Int32.Parse(localPlayer.GetPlayerTag("CusAmount")); //Player tags are not local so lets make the tag a syned var
                    AmountArray[localPlayer.playerId + 1] = CusAmount; // Save float in a array for player
                    JoinGame(1);
                }
            }
            else Debug.LogWarning("someones else is using Bar");
        }
        public void DoStuff()
        {
            // This will be sent to all clients and run locally on each one (including the one sending
            SendCustomNetworkEvent(NetworkEventTarget.All, "NetworkEventStuff");
            ///NetworkEventStuff();
        }
        public void NetworkEventStuff()
        {
            Debug.LogWarning("Networked");
            UpdateMainMenu(player1ID, player2ID);
        }

    }
}