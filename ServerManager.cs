using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Networking;

public class ServerManager : MonoBehaviour
{
    public static ServerManager instance;


    [SerializeField] private TMP_InputField nameInput, passInput;
    [SerializeField] private GameObject loginScreen;


    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    // Do whatever you need to do with this script
    //If you need additional buttons just let me know 
    //and I will create them in the editor and link
    //them to any new function you create.

    //This will go hand and hand with the matchmaking 
    //system. My original plan was to set up for NAT-punch through,
    // but if a relay system is better or easier for you thats fine.
    //once communication is established I will re-write my RPC's
    //in the networktransmission script to work through your transport layer.
    //the plan was to setup one client as the host and have join codes for players
    // to join the games or for random matchmaking the game is setup for 1v1 so it only need to couple
    //players together. 

    public void LoginButton()//functions calls when player hits login.
    {
        string name = nameInput.text;
        string pass = passInput.text;
        //Debug.Log("logging in as " + name + " with password " + pass);

        //reqeust player server data and setup local player data

        //for now bypass
        BypassLogin();
    }

    public void LogOutButton()
    {
        GameManager.instance.usersName = "";
        GameManager.instance.ownedSet = new bool[6] { true, true, true, true, true, true };
        loginScreen.SetActive(true);
        GameManager.instance.LogOut();
    }

    public void BypassLogin()
    {
        GameManager.instance.usersName = "Fallegon";
        GameManager.instance.ownedSet = new bool[6] { true, true, true, true, true, true };
        loginScreen.SetActive(false);
        GameManager.instance.LoggingIn();
    }

}
