using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkObject : NetworkBehaviour {

    NetworkManager networkManager;
    [SyncVar]
    public int connectionID = -1;
    [SyncVar]
    public bool clientReady = false;

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);

        networkManager = FindObjectOfType<NetworkManager>();

        connectionID += networkManager.numPlayers;
        this.gameObject.name = "NetworkObject " + connectionID;
    }
	
	// Update is called once per frame
	void Update () {

        if (isLocalPlayer)
        {
            if(!GameManager.Instance.startPressed && clientReady)
            {
                CmdSetReady();
                GameManager.Instance.gmClientReady = false;
            }
            if (GameManager.Instance.startPressed && !GameManager.Instance.gmClientReady)
            {
                if (!clientReady)
                {
                    CmdSetReady();
                    GameManager.Instance.gmClientReady = true;
                }
            }

            if(GameManager.Instance.everybodyReady)
            {
                CmdSetReady();
                GameManager.Instance.everybodyReady = false;
                GameManager.Instance.gmClientReady = false;
                GameManager.Instance.startPressed = false;
                if(isServer)
                    networkManager.ServerChangeScene("Game");

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
        clientReady = true;
        Debug.Log(gameObject.name + " is ready");
    }
}
