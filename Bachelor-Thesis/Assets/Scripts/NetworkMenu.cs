using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkMenu : NetworkBehaviour {
    
    #region Singleton
    public static NetworkMenu Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(transform.gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    #endregion

    NetworkObject[] nO;
    NetworkManager networkManager;

    // Use this for initialization
    void Start () {
        DontDestroyOnLoad(this);
        nO = FindObjectsOfType<NetworkObject>();
        networkManager = FindObjectOfType<NetworkManager>();
	}

    // Update is called once per frame
    void Update () {
        //if (isLocalPlayer)
        //{
            if (nO.Length != networkManager.numPlayers)
                nO = FindObjectsOfType<NetworkObject>();

            if (GameManager.Instance.gmClientReady)
            {
                Debug.Log("Looking if everybody is ready.");
                for(int i = 0; i < nO.Length; i++)
                {
                    if (!nO[i].clientReady)
                        return;
                }
                GameManager.Instance.everybodyReady = true;
            }
        //}
    }
}
