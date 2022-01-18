
using System;
using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;


public class bumpers : UdonSharpBehaviour
{
    public float ExplosionStrength = 100;
    public float ExplosionRadius = 5;
    private int[] ids = null;
    VRCPlayerApi[] players = new VRCPlayerApi[20];
    public Text PlayerDataDisplays; // Main \Screen
    private string futureCanvasText; // Main Screen *Update
    [UdonSynced]
    public float Bounce = 0;
    public  GameObject[] Bumpers = new GameObject[5];
    private float Newbounce = 0;

    private Vector3 StartingPos;

    void Start()
    {
        StartingPos = this.GetComponent<Transform>().position;
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        localPlayer.SetPlayerTag("Bounces", 0.ToString());
        DoStuff();
    }
    public void OnCollisionEnter(Collision col)
    {
        // Checker is ball hits a Bumper
        VRCPlayerApi localPlayer = Networking.LocalPlayer;
        if (col.gameObject.name == "RBumper")
        {

            //col.rigidbody.AddExplosionForce(ExplosionStrength, transform.position, ExplosionRadius);
            Bounce++;
            Newbounce = Bounce + Newbounce;
            Debug.LogWarning(col.collider.gameObject.name);
            return;
        }
        else if (col.gameObject.name == "LBumper")
        {

            //col.rigidbody.AddExplosionForce(ExplosionStrength, transform.position, ExplosionRadius);
            Bounce++;
            Newbounce = Bounce + Newbounce;
            Debug.LogWarning(col.collider.gameObject.name);
            return;
        }
        else if (col.gameObject.name == "RCBumper")
        {

           // col.rigidbody.AddExplosionForce(ExplosionStrength, transform.position, ExplosionRadius);
            Bounce++;
            Newbounce = Bounce + Newbounce;
            Debug.LogWarning(col.collider.gameObject.name);
            return;
        }
        else if (col.gameObject.name == "LCBumper")
        {

           // col.rigidbody.AddExplosionForce(ExplosionStrength, transform.position, ExplosionRadius);
            Bounce++;
            Newbounce = Bounce + Newbounce;
            Debug.LogWarning(col.collider.gameObject.name);
            return;
        }
        else if (col.gameObject.name == "MBumper")
        {
  
           // col.rigidbody.AddExplosionForce(ExplosionStrength, transform.position, ExplosionRadius);
            Bounce++;
            Newbounce = Bounce + Newbounce;
            Debug.LogWarning(col.collider.gameObject.name);
            return;
        }
        /// Checker if ball is out of play
        if (col.gameObject.name == "Bounds" )
        {
            Debug.Log("ball found");
            this.transform.position = StartingPos;
        }
        else
        localPlayer.SetPlayerTag("Bounces", Newbounce.ToString());
        DoStuff();
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
    }
    public void UpdateText()
    {
        futureCanvasText = ""; 

        // Process for each player // 
        for (int i = 0; i < ids.Length; i++)
        {
            if (ids[i] != -1)
            {
                var player = VRCPlayerApi.GetPlayerById(ids[i]);
                //PlayerDataDisplays.text += string.Format("id:{0}, name:{1} \r\n", player.playerId.ToString(), player.displayName);
                // futureCanvasText += string.Format("Name:{0},PID{1},CusAccount:${2},UID:{3},BarAccount:${4} \r\n", player.displayName, player.GetPlayerTag("PID"), player.GetPlayerTag("CusAmount"), player.playerId, BarAmount); //debug
                futureCanvasText += string.Format("Name:{0},Bounces:{1},UID:{2} \r\n", player.displayName, player.GetPlayerTag("Bounces"), player.playerId);
                PlayerDataDisplays.text = futureCanvasText.ToString();
            }
        }
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

        UpdateText();
    }
    public void DoStuff()
    {
        // This will be sent to all clients and run locally on each one (including the one sending)
        SendCustomNetworkEvent(NetworkEventTarget.All, "NetworkEventStuff");
        ///NetworkEventStuff();
    }
    public void NetworkEventStuff()
    {
        InitializeIdsIfNull();
        UpdateText();
    }
}
