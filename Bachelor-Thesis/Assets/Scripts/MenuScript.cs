using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class MenuScript : MonoBehaviour {

    public TMP_Text text;
    public TMP_Dropdown dropdown;

    NetworkManager networkManager;

    // Use this for initialization
    void Start () {
        networkManager = FindObjectOfType<NetworkManager>();
        dropdown.value = GameManager.Instance.gameMode;
	}
	
	// Update is called once per frame
	void Update () {

        if (dropdown.value != GameManager.Instance.gameMode)
            dropdown.value = GameManager.Instance.gameMode;
    }

    public void SetReady()
    {
        if (GameManager.Instance.startPressed)
        {
            text.text = "Start";
            GameManager.Instance.startPressed = false;
            return;
        }
        if(GameManager.Instance.gameMode != 0)
            text.text = "wait for others";
        GameManager.Instance.startPressed = true;
    }
    
    public void EndGame()
    {
        networkManager.StopServer();
        networkManager.StopClient();
        GameManager.Instance.EndGame();
    }

    public void SetMode(int i)
    {
        GameManager.Instance.gameMode = i;
    }
}
