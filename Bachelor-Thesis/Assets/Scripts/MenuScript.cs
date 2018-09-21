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
    [Header("SavePath")]
    public TMP_InputField savePathInputField;
    public TMP_Text savePathInputFieldPlaceholderText;
    [Header("Time")]
    public TMP_InputField timeLimitInputField;
    public TMP_Text timeLimitInputFieldPlaceholderText;
    public TMP_Text showTimerButtonText;
    [Header("Sound")]
    public Slider musicVolBar;
    public Slider sfxVolBar;
    [Header("Session Settings")]
    public TMP_Text idText;
    public TMP_Text gText;
    public TMP_Text turnText;
    public TMP_Text intervalMinText;
    public TMP_Text intervalMaxText;
    public TMP_Text amountTurnsText;
    public TMP_Text generateTasksText;
    public TMP_Text randomTaskText;
    public TMP_Text showScoreText;

    NetworkManager networkManager;

    void Awake()
    {
        GameManager.Instance.LoadOptions();

        if (!Directory.Exists(GameManager.Instance.pathToSaveLocation))
            GameManager.Instance.pathToSaveLocation = Application.persistentDataPath;

        GameManager.Instance.LoadGeneratedTasks();
    }

    // Use this for initialization
    void Start () {
        networkManager = FindObjectOfType<NetworkManager>();
        dropdown.value = GameManager.Instance.gameMode;
        musicVolBar.value = GameManager.Instance.musicVolume;
        sfxVolBar.value = GameManager.Instance.sfxVolume;

        idText.text = "" + GameManager.Instance.currentParticipantID;
        gText.text = GameManager.Instance.currentParticipantGender;
        turnText.text = "" + GameManager.Instance.currentParticipantTurn;

        intervalMinText.text = "" + GameManager.Instance.intervalMin;
        intervalMaxText.text = "" + GameManager.Instance.intervalMax;
        amountTurnsText.text = "" + GameManager.Instance.turnsToPlay;
        generateTasksText.text = "" + GameManager.Instance.taskPerTurn;
    }
	
	// Update is called once per frame
	void Update () {

        if (GameManager.Instance.showTimer && showTimerButtonText.text != "display time")
            showTimerButtonText.text = "display time";
        if (!GameManager.Instance.showTimer && showTimerButtonText.text != "<s>display time")
            showTimerButtonText.text = "<s>display time";

        if (GameManager.Instance.showScore && showScoreText.text != "display score")
            showScoreText.text = "display score";
        if (!GameManager.Instance.showScore && showScoreText.text != "<s>display score")
            showScoreText.text = "<s>display score";

        if (GameManager.Instance.randomTasks && randomTaskText.text != "random task")
            randomTaskText.text = "random task";
        if (!GameManager.Instance.randomTasks && randomTaskText.text != "<s>random task")
            randomTaskText.text = "<s>random task";

        if (savePathInputFieldPlaceholderText.text != GameManager.Instance.pathToSaveLocation && savePathInputFieldPlaceholderText.text != "error")
            savePathInputFieldPlaceholderText.text = GameManager.Instance.pathToSaveLocation;


        if (timeLimitInputFieldPlaceholderText.text != GameManager.Instance.timeLimit.ToString() && timeLimitInputFieldPlaceholderText.text != "error")
            timeLimitInputFieldPlaceholderText.text = GameManager.Instance.timeLimit.ToString();

        if (dropdown.value != GameManager.Instance.gameMode)
            dropdown.value = GameManager.Instance.gameMode;
        if (!GameManager.Instance.isHost && dropdown.interactable)
            dropdown.interactable = false;
        if (GameManager.Instance.isHost && !dropdown.interactable)
            dropdown.interactable = true;
    }
    
    #region Set Options
    public void SetSavePath(string path)
    {
        if (path.Length == 0)
            return;
        savePathInputField.text = "";

        if (Directory.Exists(path))
        {
            GameManager.Instance.pathToSaveLocation = path;
            savePathInputFieldPlaceholderText.text = GameManager.Instance.pathToSaveLocation;
        }
        else
            savePathInputFieldPlaceholderText.text = "error";
    }

    public void ShowTimer()
    {
        GameManager.Instance.showTimer = !GameManager.Instance.showTimer;
    }
    public void SetTimeLimit(string s)
    {
        if (s.Length == 0)
            return;
        GameManager.Instance.timeLimit = float.Parse(s);
        timeLimitInputFieldPlaceholderText.text = GameManager.Instance.timeLimit.ToString();
    }

    public void SetIntervallMin(string s)
    {
        if (s.Length == 0)
            return;
        GameManager.Instance.intervalMin = int.Parse(s);
        intervalMinText.text = "" + GameManager.Instance.intervalMin;
    }
    public void SetIntervallMax(string s)
    {
        if (s.Length == 0)
            return;
        GameManager.Instance.intervalMax = int.Parse(s);
        intervalMaxText.text = "" + GameManager.Instance.intervalMax;
    }

    public void SetTurnsToPlay(string s)
    {
        if (s.Length == 0)
            return;
        GameManager.Instance.taskPerTurn = int.Parse(s);
        amountTurnsText.text = "" + GameManager.Instance.turnsToPlay;
        SetTurn("0");
    }

    public void UseRandomTasks()
    {
        GameManager.Instance.randomTasks = !GameManager.Instance.randomTasks;
    }

    public void GenerateTasks()
    {
        GameManager.Instance.GenerateTasks();
    }
    public void GenerateTasks(string s)
    {
        if (s.Length == 0)
            return;
        GameManager.Instance.taskPerTurn = int.Parse(s);
        GenerateTasks();
        generateTasksText.text = "" + GameManager.Instance.taskPerTurn;
    }

    public void ShowScore()
    {
        GameManager.Instance.showScore = !GameManager.Instance.showScore;
    }

    public void SetMusicLevel(float musicLvl)
    {
        MusicManager mM = FindObjectOfType<MusicManager>();
        mM.SetMusicLevel(musicLvl);
    }
    public void SetSfxLevel(float sfxLvl)
    {
        MusicManager mM = FindObjectOfType<MusicManager>();
        mM.SetSfxLevel(sfxLvl);
    }

    public void SaveOptions()
    {
        GameManager.Instance.SaveOptions();
    }

    #region Participant
    public void SetID(string s)
    {
        if (s.Length == 0)
            return;
        GameManager.Instance.currentParticipantID = int.Parse(s);
        idText.text = "" + GameManager.Instance.currentParticipantID;
    }

    public void SetTurn(string s)
    {
        if (s.Length == 0)
            return;
        GameManager.Instance.currentParticipantTurn = int.Parse(s);
        if (GameManager.Instance.currentParticipantTurn >= GameManager.Instance.turnsToPlay)
            GameManager.Instance.currentParticipantTurn = GameManager.Instance.turnsToPlay;
        turnText.text = "" + GameManager.Instance.currentParticipantTurn;
    }

    public void SetG(string s)
    {
        if (s.Length == 0)
            return;
        GameManager.Instance.currentParticipantGender = s;
        gText.text = GameManager.Instance.currentParticipantGender;
    }
    #endregion
    #endregion

    #region Mainmenu Functions
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
    #endregion
}
