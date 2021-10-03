using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;


public class UIbuttonEvent : UdonSharpBehaviour
{
    private VRCPlayerApi _localPlayer;
    private bool _isEditor;
    private string itemName;
    public UdonSharpBehaviour target;
    public bool networked;
    //public string[] targetEvent = new string[2];
    public string targetEvent;

    private void Start()
    {
        _localPlayer = Networking.LocalPlayer;
        if (_localPlayer == null) _isEditor = true;
    }
    public override void Interact()
    {
        SendEvent();
    }

    public void SendEvent()
    {
        //for (int i = 0; i < targetEvent.Length; i++)
        if (networked && !_isEditor)
        {
            target.SendCustomNetworkEvent(NetworkEventTarget.All, targetEvent);
        }
        else
        {
            target.SendCustomEvent(targetEvent);
        }
    }
    public void ItemName()
    {
        //Get name of gameobject that being excuted from
        Debug.LogWarning(this.name);
        itemName = this.name;
        _localPlayer.SetPlayerTag("Item", itemName.ToString()); // We can acess GetplayerTag in another script
        Debug.LogWarning("AAAAAA");
        SendEvent(); // Acess Event from another script
    }

}
