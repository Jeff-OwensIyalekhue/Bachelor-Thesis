using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkObject : NetworkBehaviour {

    NetworkManager networkManager;
    NetworkMenu networkMenu;
    [SyncVar]
    public int connectionID = -1;
    [SyncVar]
    public bool clientReady;
	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);

        networkManager = FindObjectOfType<NetworkManager>();
        networkMenu = FindObjectOfType<NetworkMenu>();

        connectionID += networkManager.numPlayers;
        this.gameObject.name = "NetworkObject " + connectionID;
    }
	
	// Update is called once per frame
	void Update () {

        if (isLocalPlayer)
        {
            if (GameManager.Instance.clientReady && !GameManager.Instance.everybodyReady)
            {
                if(!clientReady)
                    CmdSetReady();
            }
            if (clientReady)
            {
                if (isServer)
                {
                    if (networkMenu.CheckIfEverybodyReady())
                    {
                        CmdSetReady();
                        GameManager.Instance.everybodyReady = true;
                    }
                }
            }
        }
    }

    [Command]
    public void CmdSetReady()
    {
        if (clientReady)
        {
            clientReady = false;
            Debug.Log(gameObject.name + " is not ready");
            return;
        }
        Debug.Log(gameObject.name + " is ready");
        clientReady = true;
    }
}
