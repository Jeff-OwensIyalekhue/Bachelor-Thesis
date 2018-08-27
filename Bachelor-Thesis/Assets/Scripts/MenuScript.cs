using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MenuScript : MonoBehaviour {
    
    NetworkManager networkManager;
    string sceneToLoad = "Game";

    // Use this for initialization
    void Start () {
        networkManager = FindObjectOfType<NetworkManager>();
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void StartGame()
    {
        networkManager.ServerChangeScene(sceneToLoad);
    }

    public void EndGame()
    {
        networkManager.StopServer();
        GameManager.Instance.EndGame();
    }

    public void SetSceneToLoad(int i)
    {
        switch (i)
        {
            case 0:
                networkManager.StartHost();
                sceneToLoad = "Game";
                break;
            default:
                Debug.Log("Default Case - invalid scene case -> set Game as scene");
                sceneToLoad = "Game";
                break;
        }
    }
}
