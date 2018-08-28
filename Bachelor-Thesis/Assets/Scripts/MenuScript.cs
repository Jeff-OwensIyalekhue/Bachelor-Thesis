using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class MenuScript : MonoBehaviour {

    public TMP_Text text;

    NetworkManager networkManager;
    string sceneToLoad = "Game";

    // Use this for initialization
    void Start () {
        networkManager = FindObjectOfType<NetworkManager>();
	}
	
	// Update is called once per frame
	void Update () {
        if (GameManager.Instance.everybodyReady)
            StartGame();
	}

    public void SetReady()
    {
        if (GameManager.Instance.startPressed)
        {
            text.text = "Start";
            GameManager.Instance.startPressed = false;
            return;
        }
        text.text = "wait for others";
        GameManager.Instance.startPressed = true;
    }

    public void StartGame()
    { 
    }

    public void EndGame()
    {
        networkManager.StopServer();
        networkManager.StopClient();
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
