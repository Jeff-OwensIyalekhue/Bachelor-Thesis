using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class MenuScript : MonoBehaviour {

    public TMP_Text startButtonText;
    public TMP_Dropdown dropdown;

    public TMP_Text savePathInputfieldPlaceholder;

    NetworkManager networkManager;

    // Use this for initialization
    void Start () {
        networkManager = FindObjectOfType<NetworkManager>();
        dropdown.value = GameManager.Instance.gameMode;
	}
	
	// Update is called once per frame
	void Update () {

        if (savePathInputfieldPlaceholder.text != GameManager.Instance.pathToSaveLocation)
            savePathInputfieldPlaceholder.text = GameManager.Instance.pathToSaveLocation;

        if (dropdown.value != GameManager.Instance.gameMode)
            dropdown.value = GameManager.Instance.gameMode;
        if (!GameManager.Instance.isHost && dropdown.interactable)
            dropdown.interactable = false;
        if (GameManager.Instance.isHost && !dropdown.interactable)
            dropdown.interactable = true;
    }

    public void SetSavePath(string path)
    {
        GameManager.Instance.pathToSaveLocation = path;
    }

    public void SetReady()
    {
        if (GameManager.Instance.startPressed)
        {
            startButtonText.text = "Start";
            GameManager.Instance.startPressed = false;
            return;
        }
        if(GameManager.Instance.gameMode != 0)
            startButtonText.text = "wait for others";
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
