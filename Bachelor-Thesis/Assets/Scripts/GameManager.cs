using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameManager{

    #region Instanziierung
    private static GameManager instance;

    private GameManager()
    {
        if (instance != null)
            return;
        instance = this;

    }

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameManager();
            }

            return instance;
        }
    }
    #endregion

    public int correctAnswers = 0;
    public int wrongAnswers = 0;
    public int skippedAnswers = 0;

    public ParticipantData participant;

    public int gameMode = 0;            // 0:= SinglePlayer; 1:= HalbCoop; 2:= Versus
    public int nGameMode = 0;
    public int enemyScore = 0;
    public bool enemyScored = false;

    public bool preGameRunning = false;
    public bool gameRunning = false;
    
    public bool isConnected = false;

    public bool startPressed = false;
    public bool gmClientReady = false;
    public bool everybodyReady = false;
    public int amountPlayerReady = 0;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void CreateUserData(string s)
    {

        string pathFolder = "I:/"+s;

        try
        {
            // Determine whether the directory exists.
            if (Directory.Exists(pathFolder))
            {
                Debug.Log("That path exists already.");
                return;
            }

            // Try to create the directory.
            DirectoryInfo di = Directory.CreateDirectory(pathFolder);
            Debug.Log("The directory was created successfully at " + Directory.GetCreationTime(pathFolder));

            // Delete the directory.
            //di.Delete();
            //Debug.Log("The directory was deleted successfully.");
        }
        catch (Exception e)
        {
            Debug.Log("The process failed: " + e.ToString());
        }
        finally { }

        // Creating a human readable .txt file of particpant data
        //Debug.Log("create user data");
        string path = pathFolder + "/" + s + "_HR.txt";
        
        using(StreamWriter file = new StreamWriter(path, true))
        {
            file.WriteLine("Particpant " + participant.identification);

            switch (participant.gameMode)
            {
                case 0:
                    file.WriteLine("Singleplayer");
                    break;
                case 1:
                    file.WriteLine("Halb Coop");
                    break;
                case 2:
                    file.WriteLine("Versus");
                    break;
                case 3:
                    file.WriteLine("Party");
                    break;
                default:
                    file.WriteLine("Unknown Mode");
                    break;
            }

            file.WriteLine(participant.timePlayed.ToString("N2") + " Seconds");

            file.WriteLine("Tasks:");
            file.WriteLine("    Overall: " + (participant.correctAnswers + participant.wrongAnswers + participant.skippedTasks).ToString());
            file.WriteLine("        Solved: " + (participant.correctAnswers + participant.wrongAnswers).ToString());
            file.WriteLine("            Correct Solutions: " + (participant.correctAnswers).ToString());
            file.WriteLine("            Wrong Solutions: " + (participant.wrongAnswers).ToString());
            file.WriteLine("    Skipped: " + participant.skippedTasks);

            file.WriteLine("TaskData:");
            int i = 1;
            foreach(TaskData task in participant.tasks)
            {
                file.WriteLine(i + ". Task **************************");
                i++;
                file.WriteLine(task.task);
                file.WriteLine("time needed: " + task.timeNeeded);
                file.WriteLine("solved: " + task.solved);
                file.WriteLine("correct answer: " + task.correctAnswer);
                file.WriteLine("Enemy score at this point: " + task.enemyScore);
                file.WriteLine("**********************************");
            }

        }

        // Creating a serialized file of particpant data
        BinaryFormatter bF = new BinaryFormatter();
        FileStream fileB = File.Create(pathFolder + "/" + s + "_NHR.dat");

        ParticipantData data = participant;

        bF.Serialize(fileB, data);
        fileB.Close();

    }

    public void LoadData()
    {

    }

    // End the game / return to the desktop
    public void EndGame()
    {
        //If we are running in a standalone build of the game
        #if UNITY_STANDALONE
            //Quit the application
            Application.Quit();
        #endif

        //If we are running in the editor
        #if UNITY_EDITOR
            //Stop playing the scene
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}

[Serializable]
public class ParticipantData
{
    public int identification;
    public int gameMode;
    public float timePlayed;
    public int skippedTasks;
    public int correctAnswers;
    public int wrongAnswers;
    public List<TaskData> tasks;

    public ParticipantData(int id, int gM)
    {
        identification = id;
        gameMode = gM;
        timePlayed = 0;
        skippedTasks = 0;
        correctAnswers = 0;
        wrongAnswers = 0;
        tasks = new List<TaskData>();
    }
}
[Serializable]
public class TaskData
{
    public float timeNeeded;
    public string task;
    public bool solved;
    public bool correctAnswer;
    public int enemyScore;

    public TaskData(float t, string ta, bool s, bool c, int e)
    {
        timeNeeded = t;
        task = ta;
        solved = s;
        correctAnswer = c;
        enemyScore = e;
    }
}
