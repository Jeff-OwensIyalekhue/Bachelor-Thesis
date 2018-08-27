using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkObject : NetworkBehaviour {

    NetworkManager networkManager;
    [SyncVar]
    public int connectionID = -1;
    [SyncVar]
    public bool ready;
	// Use this for initialization
	void Start () {
        networkManager = FindObjectOfType<NetworkManager>();
        connectionID += networkManager.numPlayers;
        this.gameObject.name = "NetworkObject" + connectionID;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (isLocalPlayer)
                CmdSetReady();
        }
    }

    [Command]
    public void CmdSetReady()
    {
        Debug.Log(gameObject.name + " is ready");
        ready = true;
    }
}
