using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkObject : NetworkBehaviour {

    NetworkManager networkManager;
    [SyncVar]
    public int id = -1;
    [SyncVar]
    public bool ready;
	// Use this for initialization
	void Start () {
        networkManager = FindObjectOfType<NetworkManager>();
        id += networkManager.numPlayers;
        //if(isLocalPlayer == true)
        //{
            if (isServer)
                this.gameObject.name = "NetworkObject" + id + "[Server]";
            else if (isClient)
                gameObject.name = "NetworkObject" + id + "[Client]";
        //}
        //else
        //{
        //    if (isClient)
        //        gameObject.name = "NetworkObject" + id + "[Client]";
        //    if (isServer)
        //        this.gameObject.name = "NetworkObject" + id + "[Server]";
        //}
    }
	
	// Update is called once per frame
	void Update () {
	}

}
