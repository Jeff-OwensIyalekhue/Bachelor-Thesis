using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkMenu : NetworkBehaviour {

    NetworkObject[] nO;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        
    }

    public bool CheckIfEverybodyReady()
    {
        nO = FindObjectsOfType<NetworkObject>();
        for (int i = 0; i < nO.Length; i++)
        {
            if (!nO[i].clientReady)
                return false; ;
        }
        GameManager.Instance.everybodyReady = true;
        return true;
    }
}
