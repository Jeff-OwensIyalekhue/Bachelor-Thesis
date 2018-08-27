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
        text.text = "wait for others";
        GameManager.Instance.clientReady = true;
    }

    public void StartGame()
    {
        GameManager.Instance.everybodyReady = false;
        GameManager.Instance.clientReady = false;
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        text.text = "3";
        yield return new WaitForSeconds(1);
        text.text = "2";
        yield return new WaitForSeconds(1);
        text.text = "1";
        yield return new WaitForSeconds(1);
        text.text = "0";
        yield return new WaitForSeconds(.1f);

        networkManager.ServerChangeScene(sceneToLoad);
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
