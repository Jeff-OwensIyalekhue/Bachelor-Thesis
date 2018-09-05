using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class MenuScript : MonoBehaviour {

    public TMP_Text startButtonText;
    public TMP_Dropdown dropdown;

    public TMP_InputField savePathInputField;
    public TMP_Text savePathInputfieldPlaceholderText;

    public TMP_Text showTimerButtonText;
    public Slider musicVolBar;

    NetworkManager networkManager;

    void Awake()
    {
        GameManager.Instance.LoadOptions();
    }

    // Use this for initialization
    void Start () {
        networkManager = FindObjectOfType<NetworkManager>();
        dropdown.value = GameManager.Instance.gameMode;
        musicVolBar.value = GameManager.Instance.musicVolume;
    }
	
	// Update is called once per frame
	void Update () {

        if (GameManager.Instance.showTimer && showTimerButtonText.text != "display time")
            showTimerButtonText.text = "display time";
        if (!GameManager.Instance.showTimer && showTimerButtonText.text != "<s>display time")
            showTimerButtonText.text = "<s>display time";

        if (savePathInputfieldPlaceholderText.text != GameManager.Instance.pathToSaveLocation && savePathInputfieldPlaceholderText.text != "error")
            savePathInputfieldPlaceholderText.text = GameManager.Instance.pathToSaveLocation;

        if (dropdown.value != GameManager.Instance.gameMode)
            dropdown.value = GameManager.Instance.gameMode;
        if (!GameManager.Instance.isHost && dropdown.interactable)
            dropdown.interactable = false;
        if (GameManager.Instance.isHost && !dropdown.interactable)
            dropdown.interactable = true;
    }

    public void SetSavePath(string path)
    {
        savePathInputField.text = "";

        if (Directory.Exists(path))
        {
            GameManager.Instance.pathToSaveLocation = path;
            savePathInputfieldPlaceholderText.text = GameManager.Instance.pathToSaveLocation;
        }
        else
            savePathInputfieldPlaceholderText.text = "error";
    }

    public void ShowTimer()
    {
        GameManager.Instance.showTimer = !GameManager.Instance.showTimer;
    }

    public void SetMusicLevel(float musicLvl)
    {
        MusicManager mM = FindObjectOfType<MusicManager>();
        mM.SetMusicLevel(musicLvl);
    }

    public void SaveOptions()
    {
        GameManager.Instance.SaveOptions();
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
