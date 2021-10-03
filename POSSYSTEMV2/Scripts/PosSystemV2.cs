
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using System.Text.RegularExpressions;
public class PosSystemV2 : UdonSharpBehaviour
{

    int _int = 0;
    private string _input = "";
    private string[] playerNames = new string[256];
    private VRCPlayerApi localPlayer;
    private int localPlayerId;
    private VRCPlayerApi[] players = new VRCPlayerApi[81];
    private int[] ids;
    private string lastType;

    public Text textDebug;
    public Text cusAccount;
    public Text cusNameBar;
    public Text cusName;
    public Text barName;
    public Text barAccount;
    public string itemName;
    public string storedreceipt;

    public float SCA; // Starting customer amount
    public float SBA; //Starting bar Amount
    [HideInInspector] private float tipper = 0f;

    private string localPlayerName;

    public GameObject KeypadBtnC;
    public GameObject KeypadBtnB;
    public Text itemTableB;
    public Text priceTableB;
    public Text totalCostTxtB;
    public Text totalCostTxtC;
    public Text cusInputText; // User input on keypad
    private string storedItem = "";

    public Text itemTableC;
    public Text priceTableC;
    public Text cusPaymentC;
    public Text cusPaymentB;

    public Text playerDataDisplays;
    public Text reciptsDisplays;

    public Text barInputText; // User input on keypad
    public Button barBtn;
    public Button cusAcceptBtn;
    public Button barAcceptBtn;
    public Button cusBtn;

    /// <summary>
    /// All synced variables
    /// </summary>
    /// 
    // private float totalCost => ;

    [UdonSynced] public string futureCanvasText;
    [UdonSynced] public string strCusName;
    [UdonSynced] public string strBarName;

    [UdonSynced] public float totalString;
    [UdonSynced] public bool cusActive;
    [UdonSynced] public bool barActive;

    [UdonSynced] public string itemString;
    [UdonSynced] public string priceString;
    [UdonSynced] public bool boolCusAccept;
    [UdonSynced] public bool boolBarAccept;
    [UdonSynced] public bool boolBarAcceptBtn;

    [UdonSynced] public float barAmount = 40000;
    [UdonSynced] public float cusInput; // Convert User input to Double
    [UdonSynced] public float barInput; // Convert User input to Double
    [UdonSynced] public float tipAmount;
    [UdonSynced] public float CusAmount;
    [UdonSynced] public string strReceipt;

    // [UdonSynced] public string StrDebug;

    /// <summary>
    /// pHONE
    /// </summary>
    public float PSelected;
    public Text Pannel;
    private bool PhoneAmt = false;
    public bool PhoneUID = false;
    [UdonSynced] public float CtSelected;

    public void Start()
    {
        localPlayer = Networking.LocalPlayer;
        localPlayerId = localPlayer.playerId;
        localPlayerName = localPlayer.displayName;
        itemString = "";
        priceString = "";
        itemTableC.text = "";
        itemTableB.text = "";
        priceTableB.text = "";
        priceTableC.text = "";
        localPlayer.SetPlayerTag("PID", 0.ToString());
        localPlayer.SetPlayerTag("Phone", 0.ToString());
        cusAcceptBtn.interactable = false;
        barAcceptBtn.interactable = false;
        boolBarAccept = false;
        boolCusAccept = false;
        cusActive = false;
        barActive = false;
        totalCostTxtB.text = "$0000.00"; ;
        totalCostTxtC.text = "$0000.00";
        cusPaymentB.text = "Pay:$0000.00";
        cusPaymentC.text = "Pay:$0000.00";
        barInputText.text = "$0000.00";
        _input = "";
        storedItem = "";
        playerDataDisplays.text = futureCanvasText.ToString();
        UpdateText();
    }
    public void Update()
    {
        if (boolCusAccept && boolBarAccept)
        {
            textDebug.text += "Finishing up transaction\n";
            SendCustomNetworkEvent(NetworkEventTarget.All, "POSNetworkedEvent");
        }
    }
    public void POSNetworkedEvent()
    {
        Final(cusInput, barInput, CusAmount);
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
        RequestSerialization();
        DoStuff();
        strReceipt = "Waiting for receipt(s)";
        if (localPlayer.isMaster) textDebug.text += localPlayer.displayName + "You are the master";
    }
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        InitializeIdsIfNull();

        for (int i = 0; i < ids.Length; i++)
        {
            if (ids[i] == player.playerId)
            {
                ids[i] = -1;
                localPlayer.SetPlayerTag("PID", 0.ToString());
                DoStuff();
                break;
            }
        }

    }
    public override void OnDeserialization()
    {
        //UPDATE POS SCREENS
        textDebug.text += "Derserilization fired, ";

        // Cus User synced variables
        cusAccount.text = "Acct:$" + SCA.ToString();
        cusName.text = strCusName;
        cusAcceptBtn.interactable = boolCusAccept;
        cusBtn.interactable = !cusActive;

        // Bar User synced variables
        barAccount.text = "Acct:$" + barAmount.ToString();
        cusNameBar.text = strCusName;
        barName.text = strBarName;
        barAcceptBtn.interactable = boolBarAcceptBtn;
        barBtn.interactable = !barActive;

        // Both users synced variables
        itemTableB.text = itemString;
        itemTableC.text = itemString;
        priceTableC.text = priceString;
        priceTableB.text = priceString;
        totalCostTxtB.text = barInput.ToString();
        totalCostTxtC.text = barInput.ToString();
        cusPaymentB.text = "$" + cusInput.ToString();
        cusPaymentC.text = "$" + cusInput.ToString();
        cusPaymentB.text = "$" + cusInput.ToString();
        reciptsDisplays.text = strReceipt;
    }
    public void UpdateText()
    {
        futureCanvasText = ""; // Set to true to see all logs
        futureCanvasText = "---Player Lsit---\r\n";
        VRCPlayerApi.GetPlayers(players);
        foreach (VRCPlayerApi player in players)

        {
            if (player == null) continue;
            futureCanvasText += string.Format("Name:{0},PID:${1}, CusAmount:{2}, BarAmount:{3}, ID#:{4}\r\n", player.displayName, player.GetPlayerTag("PID"), CusAmount, barAmount, player.playerId);
            playerDataDisplays.text = futureCanvasText.ToString();
            Debug.Log(player.displayName);
        }
        //RequestSerialization();
    }
    public void PlusCus()
    {
        if (cusActive == false)
        {
            if (localPlayer == null) return;
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
            cusInput = 0f;
            tipAmount = 0f;
            textDebug.text += " \nCus: Cus was fired";

            localPlayer.SetPlayerTag("PID", 1.ToString());
            SCA = 20000.00f;
            if (localPlayer.GetPlayerTag("NewCus") == 1.ToString())
            {
                SCA = float.Parse(localPlayer.GetPlayerTag("CusAmount"));
            }
            if (localPlayer.GetPlayerTag("PID") == 1.ToString())
            {
                // Start setting values for Customer
                cusBtn.interactable = false;
                localPlayer.SetPlayerTag("CusAmount", SCA.ToString());
                CusAmount = Int32.Parse(localPlayer.GetPlayerTag("CusAmount"));
                localPlayer.SetPlayerTag("NewCus", 1.ToString());
                cusAccount.text = "Acct:$" + SCA.ToString();
                Debug.LogWarning("PlusCus Fired");
                strCusName = localPlayerName;
                cusName.text = strCusName;
                cusNameBar.text = strCusName;
                cusActive = true;
                boolBarAccept = false;
                boolBarAcceptBtn = false;
                RequestSerialization();
                DoStuff();
            }
        }
        else Debug.LogWarning("someones else is using Cus");
    }
    // Player joins Bar button
    public void PlusBar()// Start settings values for Bar
    {
        if (barActive == false)
        {
            if (localPlayer == null) return;
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
            barInput = 0;

            textDebug.text += " \nBar: Bar was fired";

            localPlayer.SetPlayerTag("PID", 2.ToString());

            if (localPlayer.GetPlayerTag("PID") == 2.ToString())
            {
                barAccount.text = "Acct:$" + barAmount.ToString();
                Debug.LogWarning("PlusBar Fired");
                strBarName = localPlayerName;
                barName.text = strBarName;
                barActive = true;
                boolBarAcceptBtn = false;
                barBtn.interactable = false;
                boolBarAccept = false;
                RequestSerialization();
                DoStuff();
            }
        }
        else Debug.LogWarning("someones else is using Bar");
    }
    public void Clear()
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
        if (localPlayer.GetPlayerTag("PID") == 1.ToString() && KeypadBtnC.activeInHierarchy == true)

            textDebug.text += " \nCus: clearing keypad input";
            cusInput = 0;
        {
            cusInputText.text = "Cleared";
        }
        if (localPlayer.GetPlayerTag("PID") == 2.ToString() && KeypadBtnB.activeInHierarchy == false)
        {
            textDebug.text += " \nBar: Clearing Item tables";

            itemTableB.text = "";
            itemTableC.text = "";
            totalCostTxtB.text = "$0000.00";
            itemString = itemTableB.text = "";
            priceTableB.text = "";
            priceTableC.text = "";
            priceString = priceTableC.text;
            totalCostTxtC.text = "$0000.00";
            barInput = 0;
            storedItem = "";

            //string result = rgx.Replace(reciptTxt, ""); REGEX IS NOT EXPOSED IN UDON YET ;(
            // Regex rgx = new Regex(".+(?<!\n.)\n"); /
            // Debug.LogWarning(result);
        }
        if (localPlayer.GetPlayerTag("Phone") == 1.ToString())Pannel.text = "cleared";

        if (localPlayer.GetPlayerTag("Phone") == 2.ToString())Pannel.text = "cleared"; 

        else barInputText.text = "Cleared";
       
        _input = "";
        _int = 0;
        DoStuff();
        RequestSerialization();
        Debug.LogWarning("Cleared Input");
    }
    public void LeaveBar()
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
        textDebug.text += " \nBar: Left the system";

        //if (cusActive) return;
        localPlayer.SetPlayerTag("PID", 0.ToString());
        strBarName = "";
        barName.text = strBarName;
        
        barBtn.interactable = true;
        boolBarAcceptBtn = false;
        barActive = false;
        DoStuff();
        RequestSerialization();
    }
    public void LeaveCus()
    {
        if (boolCusAccept)
        {
            textDebug.text += "\nCus: Hit Accept can not leave (You are stuck now ;) ).";
            return;
        }
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
        textDebug.text += " \nCus: Left the system";

        localPlayer.SetPlayerTag("PID", 0.ToString());
        cusBtn.interactable = true;
        cusActive = false;
        strCusName = "";
        cusName.text = strCusName;
        cusNameBar.text = strCusName;
        cusAccount.text = "Acct:$0000.00";
        totalCostTxtC.text = "$0000.00";
        cusPaymentC.text = "Pay:$0000.00";
        cusPaymentB.text = "Pay:$0000.00";
        _input = "";
        barInput = 0;
        DoStuff();
        RequestSerialization();
    }
    public void Enter()
    {
        if (localPlayer.GetPlayerTag("PID") == 1.ToString() || localPlayer.GetPlayerTag("PID") == 2.ToString() || localPlayer.GetPlayerTag("Phone") == 1.ToString() || localPlayer.GetPlayerTag("Phone") == 2.ToString())
        {
            if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
            float number = 0;
            // check if Bar input is an interger, Get user tip input
            if (float.TryParse(_input, out number))
                if (float.Parse(_input) != 0)
                {
                    if (localPlayer.GetPlayerTag("PID") == 1.ToString())
                    {
                        textDebug.text += " \nCus: Pressed Enter";

                        cusInput = float.Parse(_input);
                        float NewCusAmount = CusAmount;
                        Debug.LogWarning(NewCusAmount + "  This is player Cus ammount");
                        if (NewCusAmount - cusInput < 0 || cusInput < barInput) //Make sure Cus has enough money
                        {
                            cusPaymentC.text = "Insufficient Funds";
                            cusAcceptBtn.interactable = false; //Make sure user doesn't enter a new amount less than valid amount
                            boolBarAccept = false;
                            boolBarAcceptBtn = false;
                        }
                        else
                        {
                            float debugTip = cusInput - barInput;

                            textDebug.text += " \nCus: Entered a valid tip" + debugTip ;

                            Debug.LogWarning(cusInput - barInput + " Enter: This is tip amount");
                            tipAmount = cusInput - barInput;// assume cus > bar, new value becomes tip
                            cusPaymentC.text = "$" + cusInput.ToString();
                            cusPaymentB.text = "$" + cusInput.ToString();
                            cusAcceptBtn.interactable = true;
                            barAcceptBtn.interactable = true;
                            boolBarAcceptBtn = true;
                            tipper = 0;
                        }
                        _input = ""; //Set user input to nothing
                        cusInputText.text = "0000.00";
                        DoStuff();
                        RequestSerialization();

                    }
                    else if (localPlayer.GetPlayerTag("PID") == 2.ToString()) // Bar enters custom payment amount
                    {
                        textDebug.text += "\nBar: Pressed Enter";

                        float barkeyInput = float.Parse(_input);
                        float newstoredv = barInput + barkeyInput;
                        totalCostTxtB.text = "$" + newstoredv.ToString();
                        totalCostTxtC.text = "$" + newstoredv.ToString();

                        itemString += " MISC".ToString() + "\n"; // Add new Text TO itemString
                        itemTableC.text = itemString;
                        itemTableB.text = itemString;

                        priceString +=  " " + barkeyInput.ToString() + "\n"; // Add new Text to priceString
                        priceTableC.text = priceString;
                        priceTableB.text = priceString;

                        barInputText.text = "0000.00";
                        barInput = newstoredv;
                        storedItem += " MISC: " + barkeyInput + ", ";
                        DoStuff();
                        RequestSerialization();
                    }
                    //Phone Select UID And submit
                    if (localPlayer.GetPlayerTag("Phone") == 1.ToString())
                    {
                        PSelected = float.Parse(_input);
                        localPlayer.SetPlayerTag("Phone", 3.ToString());
                        foreach (VRCPlayerApi player in players)
                        {
                            if (player != null)
                            {
                                if (player.playerId == PSelected)
                                {
                                    Pannel.text = "Name:" + player.displayName;
                                    PhoneUID = true;

                                }
                                else Pannel.text = "No player ID Found";
                            }
                        }
                        DoStuff();
                    }
                    //phone choose Transfer Amount 
                    else if (localPlayer.GetPlayerTag("Phone") == 2.ToString())
                    {
                        localPlayer.SetPlayerTag("Phone", 3.ToString());
                        CtSelected = float.Parse(_input);
                        Pannel.text = "Transfer:$ " + CtSelected;
                        PhoneAmt = true;
                        DoStuff();
                    }
                }
            _input = "";
            _int = 0;
        }
        else Debug.LogWarning("Not a valid input");
    }
    public void Accept() // Make sure bar and cus agree with payment options
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
        if (localPlayer.GetPlayerTag("PID") == 1.ToString() && boolCusAccept == false)
        {
            textDebug.text += " \nCus: Accept was fired";

            boolCusAccept = true;
            cusAcceptBtn.interactable = false;
            DoStuff();
            RequestSerialization();
        }
        else if (localPlayer.GetPlayerTag("PID") == 2.ToString() && boolBarAccept == false)
        {
            textDebug.text += " \nBar: Accept was fired";

            boolBarAccept = true;
            boolBarAcceptBtn = false;
            barAcceptBtn.interactable = false;
            DoStuff();
            RequestSerialization();
        }

    }
    public void Final(float cusInputVal, float barInputVal, float cusAmount) // Finish out payment to bar and cus
    {
        if (localPlayer.GetPlayerTag("PID") == 1.ToString() || localPlayer.GetPlayerTag("PID") == 2.ToString())
        {
            textDebug.text += "\nFinal was fired";

            //User is Cus
            if (localPlayer.GetPlayerTag("PID") == 1.ToString())
            {
                textDebug.text += "\nCus: Final was fired";
                float NewCusAmount2 = cusAmount - cusInputVal; // Customer new account amount
                cusAccount.text = "Acct:$" + NewCusAmount2.ToString();

                textDebug.text += "\nCus: Got " + cusInputVal + "removed to there account";

                Debug.Log("Cus got " + cusInputVal + "removed to their account");

                localPlayer.SetPlayerTag("CusAmount", NewCusAmount2.ToString());
                CusAmount = NewCusAmount2;
                localPlayer.SetPlayerTag("PID", 0.ToString());
                cusBtn.interactable = true;
                boolCusAccept = false;
                cusAcceptBtn.interactable = false;
                cusActive = false;
                barActive = false;
                boolBarAccept = false;
                barBtn.interactable = true;
                boolBarAcceptBtn = false;
                textDebug.text += "\n Cus: Tip Amount" + tipAmount;

                DoStuff();
                RequestSerialization();
                Start();
            }
            //User is Bar
            else if (localPlayer.GetPlayerTag("PID") == 2.ToString())
            {
                textDebug.text += "Bar: Final was fired";

                if (tipper != 0) tipAmount = barInputVal * tipper;

                barAmount += barInputVal + tipAmount; // amount added to bar account
                textDebug.text += "\nBar: Got " + cusInputVal + "added to their account";
                Debug.Log("Bar got " + barAmount + "added to there acount");
                barAccount.text = "Acct:$" + barAmount.ToString();
                Receipt(storedItem, barInput, strCusName, tipAmount, strBarName);
                localPlayer.SetPlayerTag("PID", 0.ToString());
                DoStuff();
                RequestSerialization();
                Start();
                Debug.LogWarning(tipAmount + " Final: this is the TipAmount");
            }
        }
    }
    public void OnClicked()//User acitvated a main category
    {
        int itemStage = 1;
        CategorySelect(itemStage);
    }
    public void OnClickedKey()//User activated keypad
    {
        int keypadStage = 2;
        CategorySelect(keypadStage);
    }
    [Obsolete]// only way for childCount to work for UdonSharp
    public void CategorySelect(int stageNum)
    {
        string catType = null;
        GameObject.Find("Receipt").transform.GetChild(0).gameObject.gameObject.SetActive(false);// set Receipt object false

        if (localPlayer.GetPlayerTag("PID") == 2.ToString() || localPlayer.GetPlayerTag("PID") == 1.ToString() || localPlayer.GetPlayerTag("Phone") == 1.ToString() | localPlayer.GetPlayerTag("Phone") == 2.ToString())
        {
            catType = "SubcategoriesB";
            if (stageNum == 1) catType = lastType; // Get last known category type
            else if (stageNum == 2) catType = "Keypad_buttonsBar";
            else stageNum = 0;
        }
        else return;

        if (stageNum == 0) itemName = localPlayer.GetPlayerTag("Item").ToString() + "Item"; // User clicked main category, ex Entres
        else itemName = localPlayer.GetPlayerTag("Item").ToString(); // user clicked subcategory, ex. chicken
        Debug.LogWarning(itemName + " this is ItemName");

        GameObject subCat = GameObject.Find(catType); // Find GameObject name
        Debug.LogWarning(subCat + " This is the Subcat Name");

        if (stageNum == 1 && subCat.transform.GetChild(0).gameObject.name == "Panel") // Make sure that we find children in panels not in EntresItem childs
        {
            Debug.LogWarning("Found Panel Gameobject");
            subCat = GameObject.Find("Panel"); // Make sure that we are in Panel gameobject
            Debug.LogWarning(subCat + " is new subCat");
        }

        int catCount = subCat.transform.childCount; //Get Child count
        Debug.LogWarning(catCount + " is CatCount");

        for (int i = 0; i < catCount; i++)
        {
            Debug.LogWarning(catType + " is CatType");
            if (subCat != true) return; // Safety net 
            GameObject catChild = subCat.transform.GetChild(i).gameObject; // Get childs of Category type
            string catChildName = catChild.name; // Get GameObject name of child
            GameObject itemChildTxt = catChild.transform.GetChild(0).gameObject; // Get child object of CateType first child

            if (catChildName == itemName && stageNum == 0)// Main category
            {
                Debug.LogWarning("is Found item");
                Debug.LogWarning(catChild + " is Category type");
                catChild.SetActive(true); // Set found Child active
                lastType = itemName;
            }
            else if (catChildName == itemName && stageNum == 2) // keypad
            {
                Debug.LogWarning("Keypad number found");
                Debug.LogWarning(catChild + " is Category type");
                float keypadInput = float.Parse(itemChildTxt.GetComponent<Text>().text); // Get keypad text
                KeyPadInput(keypadInput);
                break;
            }
            else if (catChildName == itemName && stageNum == 1) // Subcategory
            {
                Debug.LogWarning("Item Found");
                Debug.LogWarning(catChild + "is Category type");
                //Debug.LogWarning(Catchild.transform.hierarchyCount); Can use to get number of all childs in hierarchy, ex bones in armature
                itemString += itemChildTxt.GetComponent<Text>().text + "\n"; // Get Item Text component of child first child
                itemTableC.text = itemString;
                itemTableB.text = itemString;
                GameObject priceChildTxt2 = catChild.transform.GetChild(1).gameObject; // Get child object of CateType next child
                priceString += " " + priceChildTxt2.GetComponent<Text>().text + "\n";// Get Price Text component of child next child
                priceTableC.text = priceString;
                priceTableB.text = priceString;
                float totalPrice = 0f; // Total price 
                totalPrice = float.Parse(priceChildTxt2.GetComponent<Text>().text); // Get Price text
                totalPrice = barInput + totalPrice; // Add current input to current total price
                totalCostTxtB.text = totalPrice.ToString();
                totalCostTxtC.text = totalPrice.ToString();
                barInput = totalPrice; // Store as gloabl variable
                storedItem += itemChildTxt.GetComponent<Text>().text + ", ";
                DoStuff();
                RequestSerialization();
                break;
            }
            else
            {
                Debug.LogWarning(catChild + "is not selected category in hierarchy, setActive: False");
                if (stageNum == 0) catChild.SetActive(false); // Make sure I do not diable keypad when in use
            }
        }
        DoStuff();
        stageNum = 0;
    }
    public void KeyPadInput(float keypadNum) // User activated a keypad
    {
        Debug.LogWarning(keypadNum + " is the keypad input");
        {
            if (_input.Length < 7 && _int != 4) _input += keypadNum; // Make sure user input does not exceed max input
            if (_input.Length < 7 && _int == 4)// add a deciaml for cents
            {
                _input += ".";
                _input += keypadNum; ;
            }
            _int = _int + 1;
            if (localPlayer.GetPlayerTag("PID") == 1.ToString())
            {
                cusInputText.text = "$" + _input;
                return;
            }
            if (localPlayer.GetPlayerTag("PID") == 2.ToString())
            {
                barInputText.text = "$" + _input;
            }
            if (localPlayer.GetPlayerTag("Phone") == 1.ToString())
            {
                Pannel.text = _input;
            }
            if (localPlayer.GetPlayerTag("Phone") == 2.ToString())
            {
                Pannel.text = _input;
            }
        }
        DoStuff();
        RequestSerialization();
        
    }
    public void Tips()
    {
        itemName = localPlayer.GetPlayerTag("Item").ToString();
        tipper = float.Parse(itemName);
    }
    public void Receipt(string item, float totalPrice, string cusName, float tip, string barName)
    {
        if (storedreceipt == null || localPlayer.GetPlayerTag("PID") != 2.ToString()) return;
        if(boolBarAccept && boolCusAccept)
        {
            string currentText = "CusName: " + cusName + ", Item(s): " + item + ", Tender: " + barName + "Tip: $" + tip + "Total: $" + totalPrice + ".\n";
            storedreceipt = currentText;
            strReceipt += storedreceipt;
            reciptsDisplays.text = strReceipt;
            textDebug.text += "Main Receipt fired";
        }

        textDebug.text += "Button Receipt fired";

        foreach (Transform child in GameObject.Find("SubcategoriesB").transform)
        {
            child.gameObject.SetActive(false); // Turn off all subcategory GameObjects
            GameObject.Find("Receipt").transform.GetChild(0).gameObject.gameObject.SetActive(true);// set Receipt object active
        }
    }
    public void DebugText()
    {
        textDebug.text = " ";
    }
    public void PhoneID()
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
        _input = "";
        Debug.LogWarning("Phojne ID");
        localPlayer.SetPlayerTag("Phone", 1.ToString());
        Pannel.text = "Choose ID Number";
    }
    public void PhoneAmount()
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
        Debug.LogWarning("Phone amount");
        _input = "";
        localPlayer.SetPlayerTag("Phone", 2.ToString());
        Pannel.text = "Choose Transder amount";

    }
    public void PhoneAccept()
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
        Debug.LogWarning("Phone accept");
        if (PhoneAmt && PhoneUID && localPlayer.GetPlayerTag("Phone") == 3.ToString())
        {
            // VRCPlayerApi.GetPlayers(players);
            foreach (VRCPlayerApi player in players)
            {
                if (player != null)
                {
                    float NewTranAmount = 0;
                    float NewCusAmountt = 0;
                    if (player.playerId == PSelected)
                    {
                        Pannel.text = "Name:" + player.displayName + "\nTransfered:$ " + CtSelected;
                        if ((player.GetPlayerTag("CusAmount")) == "") player.SetPlayerTag("CusAmount", 0.ToString());// Make sure that the given user has a valid Amount
                        NewTranAmount = float.Parse(player.GetPlayerTag("CusAmount"));
                        float Amount = NewTranAmount + CtSelected;
                        player.SetPlayerTag("CusAmount", Amount.ToString());
                        NewCusAmountt = float.Parse(localPlayer.GetPlayerTag("CusAmount"));
                        Amount = NewCusAmountt - CtSelected;
                        localPlayer.SetPlayerTag("CusAmount", Amount.ToString());
                        PhoneAmt = false;
                        PhoneUID = false;
                        RequestSerialization();
                    }
                }
            }
        }
        else Debug.LogWarning("NOT VALID");
    }
    public void LeavePhone()
    {
        if (!Networking.IsOwner(gameObject)) Networking.SetOwner(localPlayer, gameObject);
        _input = "0";
        Pannel.text = "";
        Debug.LogWarning("LeavePhone ");
        localPlayer.SetPlayerTag("Phone", 0.ToString());
        PhoneAmt = false;
        PhoneUID = false;
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
        UpdateText();
    }
}
